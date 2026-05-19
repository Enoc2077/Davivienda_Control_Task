using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public class Proceso : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public Guid ProyectoId { get; set; }
        [Parameter] public string ProyectoNombre { get; set; } = "";
        [Parameter] public EventCallback OnClose { get; set; }

        public List<ProcesoModel> ProcesosDelProyecto { get; set; } = new();
        public List<TareaModel> TareasDelProceso { get; set; } = new();
        public ProcesoModel? ProcesoSeleccionado { get; set; }
        public string TextoBusquedaProceso { get; set; } = "";

        // Modales tareas
        public bool MostrarModalTarea { get; set; } = false;
        public bool MostrarModalCrearTarea { get; set; } = false;
        public TareaModel TareaSeleccionada { get; set; } = new();

        // Bitácora
        public bool MostrarBitacoraProceso { get; set; } = false;

        // Confirmar eliminación
        public bool MostrarConfirmEliminar { get; set; } = false;
        public ProcesoModel? ProcesoAEliminar { get; set; }

        // ── FIX: método para Yoopta en lugar de lambda con bloque ──
        public void OnDescripcionChanged(string valor)
        {
            if (ProcesoSeleccionado != null)
                ProcesoSeleccionado.PROC_DES = valor;
        }

        // ── FILTERED ──────────────────────────────────────────
        public IEnumerable<ProcesoModel> ProcesosFiltradosEnModal =>
            string.IsNullOrWhiteSpace(TextoBusquedaProceso)
                ? ProcesosDelProyecto
                : ProcesosDelProyecto.Where(p =>
                    p.PROC_NOM.Contains(TextoBusquedaProceso, StringComparison.OrdinalIgnoreCase));

        // ── LIFECYCLE ──────────────────────────────────────────
        protected override async Task OnParametersSetAsync()
        {
            if (ProyectoId != Guid.Empty)
            {
                ProcesoSeleccionado = new ProcesoModel { PROC_FRE = "Diario", PROC_EST = true };
                await CargarProcesos();
            }
        }

        // ── CARGA ──────────────────────────────────────────────
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

                ProcesosDelProyecto = await FiltrarProcesosParaPantalla(todosProcesos);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar procesos: {ex.Message}");
            }
        }

        private async Task<List<ProcesoModel>> FiltrarProcesosParaPantalla(List<ProcesoModel> todosProcesos)
        {
            try
            {
                var resProyecto = await Client.GetProyectos.ExecuteAsync();
                var proyecto = resProyecto.Data?.Proyectos.FirstOrDefault(p => p.Pro_ID == ProyectoId);
                if (proyecto == null) return todosProcesos;

                if (proyecto.Pro_EST == "Finalizado" || proyecto.Pro_EST == "FINALIZADO")
                    return todosProcesos.Where(p => p.PROC_EST == true).ToList();

                var activos = todosProcesos.Where(p => p.PROC_EST == true).ToList();
                var inactivos = todosProcesos
                    .Where(p => p.PROC_EST == false)
                    .OrderByDescending(p => p.PROC_FEC_CRE)
                    .Take(5).ToList();

                return activos.Concat(inactivos).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filtrando procesos: {ex.Message}");
                return todosProcesos;
            }
        }

        public async Task CargarTareasDelProceso(ProcesoModel proc)
        {
            ProcesoSeleccionado = proc;
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
                    }).ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar tareas: {ex.Message}");
            }
            StateHasChanged();
        }

        // ── GUARDAR (crear o editar) ───────────────────────────
        public async Task GuardarCambiosProceso()
        {
            if (ProcesoSeleccionado == null) return;
            try
            {
                var fechaCreacion = ProcesoSeleccionado.PROC_FEC_CRE == default
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

                if (ProcesoSeleccionado.PROC_ID == Guid.Empty)
                    await Client.InsertProceso.ExecuteAsync(input);
                else
                    await Client.UpdateProceso.ExecuteAsync(input);

                await CargarProcesos();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando proceso: {ex.Message}");
            }
        }

        // ── NUEVO PROCESO ──────────────────────────────────────
        public void PrepararNuevoProceso()
        {
            ProcesoSeleccionado = new ProcesoModel
            {
                PROC_ID = Guid.Empty,
                PROC_NOM = "",
                PROC_FRE = "Diario",
                PROC_EST = true,
                PRO_ID = ProyectoId
            };
            TareasDelProceso = new();
            StateHasChanged();
        }

        // ── ELIMINAR PROCESO ──────────────────────────────────
        public void ConfirmarEliminar(ProcesoModel proc)
        {
            ProcesoAEliminar = proc;
            MostrarConfirmEliminar = true;
            StateHasChanged();
        }

        public void CancelarEliminar()
        {
            ProcesoAEliminar = null;
            MostrarConfirmEliminar = false;
            StateHasChanged();
        }

        public async Task EjecutarEliminar()
        {
            if (ProcesoAEliminar == null) return;
            try
            {
                await Client.DeleteProceso.ExecuteAsync(ProcesoAEliminar.PROC_ID);
                Console.WriteLine($"✅ Proceso eliminado: {ProcesoAEliminar.PROC_NOM}");

                if (ProcesoSeleccionado?.PROC_ID == ProcesoAEliminar.PROC_ID)
                {
                    ProcesoSeleccionado = null;
                    TareasDelProceso = new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando proceso: {ex.Message}");
            }
            finally
            {
                ProcesoAEliminar = null;
                MostrarConfirmEliminar = false;
                await CargarProcesos();
            }
        }

        // ── TAREAS ────────────────────────────────────────────
        public void AbrirEditarTarea(TareaModel tarea)
        {
            TareaSeleccionada = tarea;
            MostrarModalTarea = true;
            StateHasChanged();
        }

        public async Task CerrarModalTarea()
        {
            MostrarModalTarea = false;

            if (TareaSeleccionada != null && TareaSeleccionada.TAR_ID != Guid.Empty)
            {
                try
                {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error guardando tarea: {ex.Message}");
                }
            }

            if (ProcesoSeleccionado != null)
            {
                await CargarTareasDelProceso(ProcesoSeleccionado);
                await VerificarYCompletarProceso(ProcesoSeleccionado.PROC_ID);
                await CargarProcesos();

                var actualizado = ProcesosDelProyecto.FirstOrDefault(p => p.PROC_ID == ProcesoSeleccionado.PROC_ID);
                if (actualizado != null) ProcesoSeleccionado = actualizado;
            }
            StateHasChanged();
        }

        public void AbrirCrearTarea()
        {
            MostrarModalCrearTarea = true;
            StateHasChanged();
        }

        public async Task CerrarModalCrearTarea()
        {
            MostrarModalCrearTarea = false;
            if (ProcesoSeleccionado != null)
            {
                await CargarTareasDelProceso(ProcesoSeleccionado);
                await CargarProcesos();
            }
            StateHasChanged();
        }

        // ── VERIFICAR COMPLETADO ──────────────────────────────
        private async Task VerificarYCompletarProceso(Guid procesoId)
        {
            try
            {
                var resTareas = await Client.GetTareas.ExecuteAsync();
                var tareas = resTareas.Data?.Tareas
                    .Where(t => t.Proc_ID == procesoId).ToList() ?? new();

                if (!tareas.Any()) return;

                if (tareas.All(t => t.Tar_EST == "Completado"))
                {
                    var resProc = await Client.GetProcesos.ExecuteAsync();
                    var proc = resProc.Data?.Procesos.FirstOrDefault(p => p.Proc_ID == procesoId);
                    if (proc == null) return;

                    await Client.UpdateProceso.ExecuteAsync(new ProcesoModelInput
                    {
                        Proc_ID = proc.Proc_ID,
                        Proc_NOM = proc.Proc_NOM,
                        Proc_DES = proc.Proc_DES,
                        Proc_FRE = proc.Proc_FRE,
                        Proc_EST = false,
                        Pro_ID = proc.Pro_ID,
                        Proc_FEC_CRE = proc.Proc_FEC_CRE,
                        Proc_FEC_MOD = DateTimeOffset.Now
                    });
                    await CargarProcesos();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando completado: {ex.Message}");
            }
        }

        // ── BITÁCORA ──────────────────────────────────────────
        public void AbrirBitacoraProceso()
        {
            MostrarBitacoraProceso = true;
            StateHasChanged();
        }

        public async Task CerrarBitacoraProceso()
        {
            MostrarBitacoraProceso = false;
            await CargarProcesos();
            StateHasChanged();
        }

        // ── HELPERS ───────────────────────────────────────────
        public void OnSearchProcesoChanged(ChangeEventArgs e)
        {
            TextoBusquedaProceso = e.Value?.ToString() ?? "";
            StateHasChanged();
        }

        public Task CerrarModal() => OnClose.InvokeAsync();
    }
}