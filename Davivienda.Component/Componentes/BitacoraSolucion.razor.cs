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
    public partial class BitacoraSolucion : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        // 🔥 LISTAS PRINCIPALES
        private List<SolucionesModel> SolucionesGlobales = new();
        private List<SolucionesModel> SolucionesFiltradas = new();
        private List<FriccionModel> FriccionesGlobales = new();

        // 🔥 DATOS PARA FILTROS
        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosList = new();
        private List<TareaModel> TareasList = new();
        private List<PrioridadModel> PrioridadesList = new();

        public SolucionesModel? SolucionSeleccionada { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarTodo();
        }

        private async Task CargarTodo()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🔵 BITÁCORA SOLUCIONES - CARGA INICIAL");
                Console.WriteLine("========================================");

                // 1. Soluciones
                var resSol = await Client.GetSoluciones.ExecuteAsync();
                SolucionesGlobales = resSol.Data?.Soluciones?.Select(s => new SolucionesModel
                {
                    SOL_ID = s.Sol_ID,
                    SOL_NOM = s.Sol_NOM,
                    SOL_DES = s.Sol_DES,
                    SOL_EST = s.Sol_EST,
                    SOL_NIV_EFE = s.Sol_NIV_EFE,
                    FRI_ID = s.Fri_ID,
                    USU_ID = s.Usu_ID,
                    SOL_FEC_CRE = s.Sol_FEC_CRE.DateTime
                }).ToList() ?? new();
                Console.WriteLine($"✅ Soluciones: {SolucionesGlobales.Count}");

                // 2. Fricciones
                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesGlobales = resFri.Data?.Fricciones?.Select(f => new FriccionModel
                {
                    FRI_ID = f.Fri_ID,
                    TAR_ID = f.Tar_ID,
                    FRI_TIP = f.Fri_TIP,
                    FRI_DES = f.Fri_DES
                }).ToList() ?? new();
                Console.WriteLine($"✅ Fricciones: {FriccionesGlobales.Count}");

                // 3. Tareas
                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasList = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    PROC_ID = t.Proc_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Tareas: {TareasList.Count}");

                // 4. Procesos
                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosList = resProc.Data?.Procesos.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Procesos: {ProcesosList.Count}");

                // 5. Proyectos
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosList = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Proyectos: {ProyectosList.Count}");

                // 6. Áreas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Áreas: {AreasList.Count}");

                // 7. Prioridades
                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                PrioridadesList = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Prioridades: {PrioridadesList.Count}");

                // 🔥 ORDENAR POR FECHA: MÁS RECIENTE PRIMERO
                SolucionesFiltradas = SolucionesGlobales
                    .OrderByDescending(s => s.SOL_FEC_CRE)
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
            Console.WriteLine($"📊 Total: {SolucionesGlobales.Count}");
            Console.WriteLine($"🎯 Activos: {filtros?.Count ?? 0}");

            if (filtros == null || !filtros.Any())
            {
                SolucionesFiltradas = SolucionesGlobales
                    .OrderByDescending(s => s.SOL_FEC_CRE)
                    .ToList();
                StateHasChanged();
                return;
            }

            var resultado = SolucionesGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                Console.WriteLine($"🔧 {filtro.Tipo} → {filtro.Etiqueta}");

                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(s =>
                            s.SOL_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Tarea":
                        var friccionesIds = FriccionesGlobales
                            .Where(f => f.TAR_ID == filtro.Id)
                            .Select(f => f.FRI_ID)
                            .ToList();
                        resultado = resultado.Where(s =>
                            s.FRI_ID.HasValue && friccionesIds.Contains(s.FRI_ID.Value));
                        break;

                    case "Proceso":
                        var tareasIds = TareasList
                            .Where(t => t.PROC_ID == filtro.Id)
                            .Select(t => t.TAR_ID)
                            .ToList();
                        var friIds = FriccionesGlobales
                            .Where(f => f.TAR_ID.HasValue && tareasIds.Contains(f.TAR_ID.Value))
                            .Select(f => f.FRI_ID)
                            .ToList();
                        resultado = resultado.Where(s =>
                            s.FRI_ID.HasValue && friIds.Contains(s.FRI_ID.Value));
                        break;

                    case "Proyecto":
                        var procesosIds = ProcesosList
                            .Where(p => p.PRO_ID == filtro.Id)
                            .Select(p => p.PROC_ID)
                            .ToList();
                        var tIds = TareasList
                            .Where(t => t.PROC_ID.HasValue && procesosIds.Contains(t.PROC_ID.Value))
                            .Select(t => t.TAR_ID)
                            .ToList();
                        var fIds = FriccionesGlobales
                            .Where(f => f.TAR_ID.HasValue && tIds.Contains(f.TAR_ID.Value))
                            .Select(f => f.FRI_ID)
                            .ToList();
                        resultado = resultado.Where(s =>
                            s.FRI_ID.HasValue && fIds.Contains(s.FRI_ID.Value));
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
                        var tarIds = TareasList
                            .Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value))
                            .Select(t => t.TAR_ID)
                            .ToList();
                        var fricIds = FriccionesGlobales
                            .Where(f => f.TAR_ID.HasValue && tarIds.Contains(f.TAR_ID.Value))
                            .Select(f => f.FRI_ID)
                            .ToList();
                        resultado = resultado.Where(s =>
                            s.FRI_ID.HasValue && fricIds.Contains(s.FRI_ID.Value));
                        break;

                    case "Prioridad":
                        resultado = resultado.Where(s =>
                            s.SOL_EST.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                }

                Console.WriteLine($"   → Quedan: {resultado.Count()}");
            }

            // 🔥 ORDENAR POR FECHA: MÁS RECIENTE PRIMERO
            SolucionesFiltradas = resultado
                .OrderByDescending(s => s.SOL_FEC_CRE)
                .ToList();

            Console.WriteLine($"📊 FINAL: {SolucionesFiltradas.Count}\n");
            StateHasChanged();
        }

        private void SeleccionarSolucion(SolucionesModel sol)
        {
            SolucionSeleccionada = sol;
            Console.WriteLine($"✅ Seleccionada: {sol.SOL_NOM}");
            StateHasChanged();
        }

        private void AbrirDetalleSolucion(SolucionesModel sol)
        {
            SolucionSeleccionada = sol;
            Console.WriteLine($"🔍 Abriendo detalle: {sol.SOL_NOM}");
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            SolucionSeleccionada = null;
            StateHasChanged();
        }

        private string ObtenerNombreTarea(Guid? friccionId)
        {
            if (!friccionId.HasValue) return "Sin tarea vinculada";

            var friccion = FriccionesGlobales.FirstOrDefault(f => f.FRI_ID == friccionId.Value);
            if (friccion == null || !friccion.TAR_ID.HasValue) return "Tarea no encontrada";

            var tarea = TareasList.FirstOrDefault(t => t.TAR_ID == friccion.TAR_ID.Value);
            return tarea?.TAR_NOM ?? "Nombre no disponible";
        }

        private void VerTareaPadre()
        {
            // TODO: Implementar navegación a la tarea padre
            Console.WriteLine("🔗 Ver Tarea Padre");
        }

        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}