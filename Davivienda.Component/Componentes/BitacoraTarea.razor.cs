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
    public partial class BitacoraTarea : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        // 🔥 LISTAS PRINCIPALES
        private List<TareaModel> TareasGlobales = new();
        private List<TareaModel> TareasFiltradas = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<ProyectosModel> ProyectosGlobales = new();

        // 🔥 DATOS PARA FILTROS
        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosList = new();
        private List<PrioridadModel> PrioridadesList = new();

        public TareaModel? TareaSeleccionada { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarTodo();
        }

        private async Task CargarTodo()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🕒 BITÁCORA TAREAS - CARGA INICIAL");
                Console.WriteLine("========================================");

                // 1. Procesos (TODOS - necesitamos verificar PROC_EST)
                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resProc.Data?.Procesos?.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PROC_DES = p.Proc_DES,
                    PROC_EST = p.Proc_EST,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Procesos: {ProcesosGlobales.Count}");

                // 2. Tareas (TODAS - filtraremos después)
                var resTar = await Client.GetTareas.ExecuteAsync();
                var todasLasTareas = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_DES = t.Tar_DES,
                    TAR_EST = t.Tar_EST,
                    TAR_FEC_INI = t.Tar_FEC_INI.DateTime,
                    TAR_FEC_FIN = t.Tar_FEC_FIN?.DateTime,
                    PROC_ID = t.Proc_ID,
                    PRI_ID = t.Pri_ID,
                    USU_ID = t.Usu_ID,
                    TAR_FEC_CRE = t.Tar_FEC_CRE.DateTime
                }).ToList() ?? new();
                Console.WriteLine($"✅ Tareas totales: {todasLasTareas.Count}");

                // 🔥 3. FILTRAR: TAR_EST = "Completado" Y PROC_EST = false
                TareasGlobales = todasLasTareas.Where(t =>
                {
                    // Debe estar completada
                    if (t.TAR_EST != "Completado") return false;

                    // Debe tener proceso
                    if (!t.PROC_ID.HasValue) return false;

                    // El proceso debe estar INACTIVO (false)
                    var proceso = ProcesosGlobales.FirstOrDefault(p => p.PROC_ID == t.PROC_ID.Value);
                    if (proceso == null) return false;

                    return proceso.PROC_EST == false;

                }).ToList();

                Console.WriteLine($"✅ Tareas en historial (Completadas + Proceso Inactivo): {TareasGlobales.Count}");

                // 4. Proyectos
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosGlobales = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Proyectos: {ProyectosGlobales.Count}");

                // 5. Áreas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Áreas: {AreasList.Count}");

                // 6. Prioridades
                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                PrioridadesList = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Prioridades: {PrioridadesList.Count}");

                // Para filtros
                ProyectosList = ProyectosGlobales;
                ProcesosList = ProcesosGlobales;

                // 🔥 ORDENAR POR FECHA: MÁS RECIENTE PRIMERO
                TareasFiltradas = TareasGlobales
                    .OrderByDescending(t => t.TAR_FEC_CRE)
                    .ToList();

                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
            }
        }

        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            Console.WriteLine("\n🔍 APLICANDO FILTROS");
            Console.WriteLine($"📊 Total: {TareasGlobales.Count}");
            Console.WriteLine($"🎯 Activos: {filtros?.Count ?? 0}");

            if (filtros == null || !filtros.Any())
            {
                TareasFiltradas = TareasGlobales
                    .OrderByDescending(t => t.TAR_FEC_CRE)
                    .ToList();
                StateHasChanged();
                return;
            }

            var resultado = TareasGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                Console.WriteLine($"🔧 {filtro.Tipo} → {filtro.Etiqueta}");

                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(t =>
                            t.TAR_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Tarea":
                        resultado = resultado.Where(t => t.TAR_ID == filtro.Id);
                        break;

                    case "Proceso":
                        resultado = resultado.Where(t => t.PROC_ID == filtro.Id);
                        break;

                    case "Proyecto":
                        var procesosIds = ProcesosList
                            .Where(p => p.PRO_ID == filtro.Id)
                            .Select(p => p.PROC_ID)
                            .ToList();
                        resultado = resultado.Where(t =>
                            t.PROC_ID.HasValue && procesosIds.Contains(t.PROC_ID.Value));
                        break;

                    case "Area":
                        var proyIds = ProyectosList
                            .Where(p => p.ARE_ID == filtro.Id)
                            .Select(p => p.PRO_ID)
                            .ToList();
                        var procIds = ProcesosList
                            .Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value))
                            .Select(p => p.PROC_ID)
                            .ToList();
                        resultado = resultado.Where(t =>
                            t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value));
                        break;

                    case "Prioridad":
                        resultado = resultado.Where(t => t.PRI_ID == filtro.Id);
                        break;
                }

                Console.WriteLine($"   → Quedan: {resultado.Count()}");
            }

            // 🔥 ORDENAR POR FECHA: MÁS RECIENTE PRIMERO
            TareasFiltradas = resultado
                .OrderByDescending(t => t.TAR_FEC_CRE)
                .ToList();

            Console.WriteLine($"📊 FINAL: {TareasFiltradas.Count}\n");
            StateHasChanged();
        }

        private void AbrirDetalleTarea(TareaModel tarea)
        {
            TareaSeleccionada = tarea;
            Console.WriteLine($"🔍 Abriendo detalle: {tarea.TAR_NOM}");
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            TareaSeleccionada = null;
            StateHasChanged();
        }

        private string ObtenerNombreProceso(Guid? procesoId)
        {
            if (!procesoId.HasValue) return "Sin proceso";
            var proceso = ProcesosGlobales.FirstOrDefault(p => p.PROC_ID == procesoId.Value);
            return proceso?.PROC_NOM ?? "Proceso no encontrado";
        }

        private string ObtenerNombreProyecto(Guid? procesoId)
        {
            if (!procesoId.HasValue) return "Sin proyecto";

            var proceso = ProcesosGlobales.FirstOrDefault(p => p.PROC_ID == procesoId.Value);
            if (proceso == null || !proceso.PRO_ID.HasValue) return "Proyecto no encontrado";

            var proyecto = ProyectosGlobales.FirstOrDefault(p => p.PRO_ID == proceso.PRO_ID.Value);
            return proyecto?.PRO_NOM ?? "Proyecto no encontrado";
        }

        private string ObtenerNombrePrioridad(Guid? prioridadId)
        {
            if (!prioridadId.HasValue) return "Sin prioridad";
            var prioridad = PrioridadesList.FirstOrDefault(p => p.PRI_ID == prioridadId.Value);
            return prioridad?.PRI_NOM ?? "Baja";
        }

        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}