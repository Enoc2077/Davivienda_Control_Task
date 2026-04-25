using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class BitacoraProceso : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public Guid ProyectoId { get; set; }

        // 🔥 LISTAS PRINCIPALES
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<ProcesoModel> ProcesosFiltrados = new();
        private List<ProyectosModel> ProyectosGlobales = new();
        private List<TareaModel> TareasGlobales = new();
        private List<TareaModel> TareasDeProceso = new();

        // 🔥 DATOS PARA FILTROS
        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();

        public ProcesoModel? ProcesoSeleccionado { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarTodo();
        }

        private async Task CargarTodo()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("📦 BITÁCORA PROCESOS - CARGA INICIAL");
                Console.WriteLine("========================================");

                // 1. Proyectos
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosGlobales = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    PRO_EST = p.Pro_EST,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Proyectos: {ProyectosGlobales.Count}");

                // 2. Procesos (TODOS)
                var resProc = await Client.GetProcesos.ExecuteAsync();
                var todosProcesos = resProc.Data?.Procesos.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PROC_DES = p.Proc_DES,
                    PROC_FRE = p.Proc_FRE ?? "Sin frecuencia",
                    PROC_EST = p.Proc_EST,
                    PRO_ID = p.Pro_ID,
                    PROC_FEC_CRE = p.Proc_FEC_CRE.DateTime,
                    PROC_FEC_MOD = p.Proc_FEC_MOD?.DateTime
                }).ToList() ?? new();
                Console.WriteLine($"✅ Procesos totales: {todosProcesos.Count}");

                // 3. Tareas
                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasGlobales = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_EST = t.Tar_EST,
                    PROC_ID = t.Proc_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Tareas: {TareasGlobales.Count}");

                // 4. Áreas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Áreas: {AreasList.Count}");

                ProyectosList = ProyectosGlobales;

                // 🔥 APLICAR LÓGICA DE BITÁCORA
                ProcesosGlobales = AplicarLogicaBitacora(todosProcesos);

                Console.WriteLine($"✅ Procesos en historial: {ProcesosGlobales.Count}");

                // 🔥 ORDENAR POR FECHA: MÁS RECIENTE PRIMERO
                ProcesosFiltrados = ProcesosGlobales
                    .OrderByDescending(p => p.PROC_FEC_CRE)
                    .ToList();

                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
            }
        }

        // 🔥 LÓGICA PRINCIPAL: DETERMINAR QUÉ PROCESOS VAN A BITÁCORA
        private List<ProcesoModel> AplicarLogicaBitacora(List<ProcesoModel> todosProcesos)
        {
            var procesosEnBitacora = new List<ProcesoModel>();

            // Agrupar por proyecto
            var procesosPorProyecto = todosProcesos.GroupBy(p => p.PRO_ID);

            foreach (var grupo in procesosPorProyecto)
            {
                var proyectoId = grupo.Key;
                if (!proyectoId.HasValue) continue;

                // Obtener proyecto
                var proyecto = ProyectosGlobales.FirstOrDefault(p => p.PRO_ID == proyectoId.Value);
                if (proyecto == null) continue;

                var procesosDelProyecto = grupo.ToList();

                // 🔥 CASO 1: PROYECTO FINALIZADO
                if (proyecto.PRO_EST == "Finalizado")
                {
                    Console.WriteLine($"📁 Proyecto '{proyecto.PRO_NOM}' FINALIZADO");
                    // TODOS los procesos inactivos van a bitácora
                    var inactivos = procesosDelProyecto.Where(p => p.PROC_EST == false).ToList();
                    procesosEnBitacora.AddRange(inactivos);
                    Console.WriteLine($"   → {inactivos.Count} procesos completados en bitácora");
                }
                // 🔥 CASO 2: PROYECTO ACTIVO - REGLA DE 5 INACTIVOS
                else
                {
                    Console.WriteLine($"📁 Proyecto '{proyecto.PRO_NOM}' ACTIVO");
                    // Solo procesos inactivos (completados)
                    var inactivos = procesosDelProyecto
                        .Where(p => p.PROC_EST == false)
                        .OrderByDescending(p => p.PROC_FEC_CRE)
                        .ToList();

                    Console.WriteLine($"   → Total inactivos: {inactivos.Count}");

                    // Si hay MÁS de 5, los excedentes van a bitácora
                    if (inactivos.Count > 5)
                    {
                        // Los 5 más recientes se quedan en pantalla
                        // Los demás van a bitácora
                        var excedentes = inactivos.Skip(5).ToList();
                        procesosEnBitacora.AddRange(excedentes);
                        Console.WriteLine($"   → {excedentes.Count} excedentes en bitácora");
                    }
                    else
                    {
                        Console.WriteLine($"   → No hay excedentes (≤ 5)");
                    }
                }
            }

            return procesosEnBitacora;
        }

        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            Console.WriteLine("\n🔍 APLICANDO FILTROS");
            Console.WriteLine($"📊 Total: {ProcesosGlobales.Count}");
            Console.WriteLine($"🎯 Activos: {filtros?.Count ?? 0}");

            if (filtros == null || !filtros.Any())
            {
                ProcesosFiltrados = ProcesosGlobales
                    .OrderByDescending(p => p.PROC_FEC_CRE)
                    .ToList();
                StateHasChanged();
                return;
            }

            var resultado = ProcesosGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                Console.WriteLine($"🔧 {filtro.Tipo} → {filtro.Etiqueta}");

                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(p =>
                            p.PROC_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Proceso":
                        resultado = resultado.Where(p => p.PROC_ID == filtro.Id);
                        break;

                    case "Proyecto":
                        resultado = resultado.Where(p => p.PRO_ID == filtro.Id);
                        break;

                    case "Area":
                        var proyIds = ProyectosList
                            .Where(p => p.ARE_ID == filtro.Id)
                            .Select(p => p.PRO_ID)
                            .ToList();
                        resultado = resultado.Where(p =>
                            p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value));
                        break;
                }

                Console.WriteLine($"   → Quedan: {resultado.Count()}");
            }

            // 🔥 ORDENAR POR FECHA: MÁS RECIENTE PRIMERO
            ProcesosFiltrados = resultado
                .OrderByDescending(p => p.PROC_FEC_CRE)
                .ToList();

            Console.WriteLine($"📊 FINAL: {ProcesosFiltrados.Count}\n");
            StateHasChanged();
        }

        private async Task AbrirDetalleProceso(ProcesoModel proceso)
        {
            ProcesoSeleccionado = proceso;

            // Cargar tareas del proceso
            TareasDeProceso = TareasGlobales
                .Where(t => t.PROC_ID == proceso.PROC_ID)
                .ToList();

            Console.WriteLine($"🔍 Abriendo detalle: {proceso.PROC_NOM}");
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            ProcesoSeleccionado = null;
            TareasDeProceso = new();
            StateHasChanged();
        }

        private string ObtenerNombreProyecto(Guid? proyectoId)
        {
            if (!proyectoId.HasValue) return "Sin proyecto";
            var proyecto = ProyectosGlobales.FirstOrDefault(p => p.PRO_ID == proyectoId.Value);
            return proyecto?.PRO_NOM ?? "Proyecto no encontrado";
        }

        private int ObtenerCantidadTareas(Guid procesoId)
        {
            return TareasGlobales.Count(t => t.PROC_ID == procesoId);
        }

        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}