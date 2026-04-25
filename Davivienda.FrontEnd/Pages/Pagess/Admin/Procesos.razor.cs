using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public class ProcesoBase : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public Guid ProyectoId { get; set; }
        [Parameter] public string ProyectoNombre { get; set; } = "";
        [Parameter] public EventCallback OnClose { get; set; }

        public List<ProcesoModel> ProcesosDelProyecto { get; set; } = new();
        public List<TareaModel> TareasDelProceso { get; set; } = new();
        public ProcesoModel? ProcesoSeleccionado { get; set; }
        public string TextoBusquedaProceso { get; set; } = "";

        // --- ESTADOS PARA MODALES DE TAREAS ---
        public bool MostrarModalTarea { get; set; } = false;
        public bool MostrarModalCrearTarea { get; set; } = false;
        public TareaModel TareaSeleccionada { get; set; } = new();

        // 🔥 NUEVO: BITÁCORA DE PROCESOS
        public bool MostrarBitacoraProceso { get; set; } = false;

        // Métodos para Editar
        public void AbrirEditarTarea(TareaModel tarea)
        {
            TareaSeleccionada = tarea;
            MostrarModalTarea = true;
            StateHasChanged();
        }

        public async Task CerrarModalTarea()
        {
            MostrarModalTarea = false;

            // 🔥 PASO 0: GUARDAR CAMBIOS DE LA TAREA AUTOMÁTICAMENTE
            if (TareaSeleccionada != null && TareaSeleccionada.TAR_ID != Guid.Empty)
            {
                try
                {
                    Console.WriteLine($"💾 Guardando tarea: {TareaSeleccionada.TAR_NOM} → Estado: {TareaSeleccionada.TAR_EST}");

                    var tareaInput = new TareaModelInput
                    {
                        Tar_ID = TareaSeleccionada.TAR_ID,
                        Tar_NOM = TareaSeleccionada.TAR_NOM,
                        Tar_DES = TareaSeleccionada.TAR_DES,
                        Tar_EST = TareaSeleccionada.TAR_EST,
                        Tar_FEC_INI = TareaSeleccionada.TAR_FEC_INI,
                        Tar_FEC_FIN = TareaSeleccionada.TAR_FEC_FIN,
                        Proc_ID = TareaSeleccionada.PROC_ID,
                        Pri_ID = TareaSeleccionada.PRI_ID,
                        Usu_ID = TareaSeleccionada.USU_ID,
                        Tar_FEC_CRE = TareaSeleccionada.TAR_FEC_CRE,
                        Tar_FEC_MOD = DateTimeOffset.Now
                    };

                    await Client.UpdateTarea.ExecuteAsync(tareaInput);
                    Console.WriteLine($"✅ Tarea guardada correctamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error guardando tarea: {ex.Message}");
                }
            }

            // 🔥 ACTUALIZACIÓN EN TIEMPO REAL
            if (ProcesoSeleccionado != null)
            {
                // 1. Recargar tareas del proceso actual
                await CargarTareasDelProceso(ProcesoSeleccionado);

                // 2. Verificar si todas las tareas están completadas
                await VerificarYCompletarProceso(ProcesoSeleccionado.PROC_ID);

                // 3. Recargar lista completa de procesos
                await CargarProcesos();

                // 4. Refrescar el proceso seleccionado con datos actualizados
                var procesoActualizado = ProcesosDelProyecto.FirstOrDefault(p => p.PROC_ID == ProcesoSeleccionado.PROC_ID);
                if (procesoActualizado != null)
                {
                    ProcesoSeleccionado = procesoActualizado;
                }

                Console.WriteLine("✅ Actualización en tiempo real completada");
            }

            StateHasChanged();
        }

        // Métodos para Crear
        public void AbrirCrearTarea()
        {
            MostrarModalCrearTarea = true;
            StateHasChanged();
        }

        public async Task CerrarModalCrearTarea()
        {
            MostrarModalCrearTarea = false;

            // 🔥 ACTUALIZACIÓN EN TIEMPO REAL
            if (ProcesoSeleccionado != null)
            {
                // 1. Recargar tareas del proceso
                await CargarTareasDelProceso(ProcesoSeleccionado);

                // 2. Recargar procesos
                await CargarProcesos();

                Console.WriteLine("✅ Tareas actualizadas después de crear");
            }

            StateHasChanged();
        }

        // 🔥 BITÁCORA DE PROCESOS
        public void AbrirBitacoraProceso()
        {
            MostrarBitacoraProceso = true;
            Console.WriteLine("📖 Abriendo Bitácora de Procesos");
        }

        public async Task CerrarBitacoraProceso()
        {
            MostrarBitacoraProceso = false;
            await CargarProcesos(); // Recargar procesos por si hubo cambios
            Console.WriteLine("❌ Cerrando Bitácora de Procesos");
            StateHasChanged();
        }

        // 🔥 VERIFICAR SI TODAS LAS TAREAS ESTÁN COMPLETADAS
        private async Task VerificarYCompletarProceso(Guid procesoId)
        {
            try
            {
                // Cargar todas las tareas del proceso
                var resTareas = await Client.GetTareas.ExecuteAsync();
                var tareasDelProceso = resTareas.Data?.Tareas
                    .Where(t => t.Proc_ID == procesoId)
                    .ToList() ?? new();

                if (!tareasDelProceso.Any())
                {
                    Console.WriteLine($"⚠️ Proceso {procesoId} sin tareas");
                    return;
                }

                // Verificar si TODAS están completadas
                bool todasCompletadas = tareasDelProceso.All(t => t.Tar_EST == "Completado");

                if (todasCompletadas)
                {
                    Console.WriteLine($"✅ TODAS las tareas completadas. Marcando proceso como INACTIVO");

                    // Obtener el proceso actual
                    var resProc = await Client.GetProcesos.ExecuteAsync();
                    var proceso = resProc.Data?.Procesos.FirstOrDefault(p => p.Proc_ID == procesoId);

                    if (proceso != null)
                    {
                        // Actualizar proceso a INACTIVO
                        var input = new ProcesoModelInput
                        {
                            Proc_ID = proceso.Proc_ID,
                            Proc_NOM = proceso.Proc_NOM,
                            Proc_DES = proceso.Proc_DES,
                            Proc_FRE = proceso.Proc_FRE,
                            Proc_EST = false, // 🔥 MARCAR COMO INACTIVO
                            Pro_ID = proceso.Pro_ID,
                            Proc_FEC_CRE = proceso.Proc_FEC_CRE,
                            Proc_FEC_MOD = DateTimeOffset.Now
                        };

                        await Client.UpdateProceso.ExecuteAsync(input);
                        Console.WriteLine($"✅ Proceso marcado como INACTIVO automáticamente");

                        // Recargar procesos
                        await CargarProcesos();
                    }
                }
                else
                {
                    Console.WriteLine($"📝 Aún hay tareas pendientes o en progreso");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error verificando completado: {ex.Message}");
            }
        }

        // ---------------------------------------

        public IEnumerable<ProcesoModel> ProcesosFiltradosEnModal =>
            string.IsNullOrWhiteSpace(TextoBusquedaProceso)
                ? ProcesosDelProyecto
                : ProcesosDelProyecto.Where(p => p.PROC_NOM.Contains(TextoBusquedaProceso, StringComparison.OrdinalIgnoreCase));

        protected override async Task OnParametersSetAsync()
        {
            if (ProyectoId != Guid.Empty)
            {
                ProcesoSeleccionado = new ProcesoModel { PROC_FRE = "Diario", PROC_EST = true };
                await CargarProcesos();
            }
        }

        public async Task CargarProcesos()
        {
            try
            {
                var res = await Client.GetProcesos.ExecuteAsync();
                var todosProcesos = res.Data?.Procesos
                    .Where(p => p.Pro_ID == ProyectoId)
                    .Select(p => new ProcesoModel
                    {
                        PROC_ID = p.Proc_ID,
                        PROC_NOM = p.Proc_NOM,
                        PROC_DES = p.Proc_DES,
                        PROC_FRE = p.Proc_FRE ?? "Frecuencia no definida",
                        PROC_EST = p.Proc_EST,
                        PRO_ID = p.Pro_ID,
                        PROC_FEC_CRE = p.Proc_FEC_CRE,
                        PROC_FEC_MOD = p.Proc_FEC_MOD
                    }).ToList() ?? new();

                // 🔥 APLICAR FILTRO: Solo mostrar procesos según reglas
                ProcesosDelProyecto = await FiltrarProcesosParaPantalla(todosProcesos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar procesos: {ex.Message}");
            }
        }

        // 🔥 FILTRAR PROCESOS PARA PANTALLA (EXCLUIR LOS DE BITÁCORA)
        private async Task<List<ProcesoModel>> FiltrarProcesosParaPantalla(List<ProcesoModel> todosProcesos)
        {
            try
            {
                // Obtener información del proyecto
                var resProyecto = await Client.GetProyectos.ExecuteAsync();
                var proyecto = resProyecto.Data?.Proyectos.FirstOrDefault(p => p.Pro_ID == ProyectoId);

                if (proyecto == null) return todosProcesos;

                // 🔥 SI PROYECTO FINALIZADO: No mostrar procesos completados
                if (proyecto.Pro_EST == "Finalizado")
                {
                    Console.WriteLine($"📁 Proyecto '{proyecto.Pro_NOM}' FINALIZADO - Ocultando completados");
                    return todosProcesos.Where(p => p.PROC_EST == true).ToList();
                }

                // 🔥 PROYECTO ACTIVO: Mostrar TODOS activos + Solo 5 inactivos más recientes
                var activos = todosProcesos.Where(p => p.PROC_EST == true).ToList();

                var inactivos = todosProcesos
                    .Where(p => p.PROC_EST == false)
                    .OrderByDescending(p => p.PROC_FEC_CRE)
                    .Take(5) // Solo los 5 más recientes
                    .ToList();

                Console.WriteLine($"📊 Mostrando: {activos.Count} activos + {inactivos.Count} inactivos");

                return activos.Concat(inactivos).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error filtrando procesos: {ex.Message}");
                return todosProcesos;
            }
        }

        public async Task CargarTareasDelProceso(ProcesoModel proc)
        {
            ProcesoSeleccionado = proc; // Guardamos referencia directa del proceso seleccionado
            try
            {
                var res = await Client.GetTareas.ExecuteAsync();
                TareasDelProceso = res.Data?.Tareas
                    .Where(t => t.Proc_ID == proc.PROC_ID)
                    .Select(t => new TareaModel
                    {
                        TAR_ID = t.Tar_ID,
                        TAR_NOM = t.Tar_NOM,
                        TAR_DES = t.Tar_DES,
                        TAR_EST = t.Tar_EST,
                        PROC_ID = t.Proc_ID
                    })
                    .ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar tareas: {ex.Message}");
            }
        }

        public async Task GuardarCambiosProceso()
        {
            if (ProcesoSeleccionado == null) return;
            try
            {
                DateTimeOffset fechaCreacion = ProcesoSeleccionado.PROC_FEC_CRE == default
                    ? DateTimeOffset.Now
                    : ProcesoSeleccionado.PROC_FEC_CRE;

                var input = new ProcesoModelInput
                {
                    Proc_ID = ProcesoSeleccionado.PROC_ID,
                    Proc_NOM = ProcesoSeleccionado.PROC_NOM,
                    Proc_DES = ProcesoSeleccionado.PROC_DES,
                    Proc_FRE = ProcesoSeleccionado.PROC_FRE,
                    Proc_EST = ProcesoSeleccionado.PROC_EST ?? true,
                    Pro_ID = ProyectoId,
                    Proc_FEC_CRE = fechaCreacion,
                    Proc_FEC_MOD = DateTimeOffset.Now
                };

                if (ProcesoSeleccionado.PROC_ID == Guid.Empty) await Client.InsertProceso.ExecuteAsync(input);
                else await Client.UpdateProceso.ExecuteAsync(input);

                await CargarProcesos();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        public void PrepararNuevoProceso()
        {
            ProcesoSeleccionado = new ProcesoModel { PROC_ID = Guid.Empty, PROC_NOM = "", PROC_FRE = "Diario", PROC_EST = true, PRO_ID = ProyectoId };
            TareasDelProceso = new();
            StateHasChanged();
        }

        public void OnSearchProcesoChanged(ChangeEventArgs e) => TextoBusquedaProceso = e.Value?.ToString() ?? "";
        public Task CerrarModal() => OnClose.InvokeAsync();
    }
}