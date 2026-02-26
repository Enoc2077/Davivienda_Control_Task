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
    public partial class BitacoraFriccion : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<Guid> OnVerTarea { get; set; } // 🔥 Navegación a Tarea

        private List<FriccionModel> FriccionesGlobales = new();
        private List<FriccionModel> FriccionesFiltradas = new();
        private List<AreasModel> AreasList = new(); // 🔥 AÑADIDO
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<TareaModel> TareasGlobales = new();
        private List<PrioridadModel> PrioridadesList = new(); // 🔥 AÑADIDO

        private List<ProcesoModel> ProcesosFiltrados = new();
        private List<TareaModel> TareasFiltradas = new();

        public string BusquedaNombre { get; set; } = "";
        private FriccionModel? FriccionSeleccionada;
        private Dictionary<Guid, string> ColoresTareas = new();
        private string[] PaletaColores = { "#ef4444", "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6" };

        protected override async Task OnInitializedAsync() => await CargarDatos();

        private async Task CargarDatos()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🔵 BITÁCORA FRICCIONES - CARGA INICIAL");
                Console.WriteLine("========================================");

                // 1. Áreas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Áreas: {AreasList.Count}");

                // 2. Proyectos
                var resPro = await Client.GetProyectos.ExecuteAsync();
                ProyectosList = resPro.Data?.Proyectos?.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    ARE_ID = p.Are_ID // 🔥 Link a Área
                }).ToList() ?? new();
                Console.WriteLine($"✅ Proyectos: {ProyectosList.Count}");

                // 3. Procesos
                var resPrc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resPrc.Data?.Procesos?.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Procesos: {ProcesosGlobales.Count}");

                // 4. Tareas
                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasGlobales = resTar.Data?.Tareas?.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    PROC_ID = t.Proc_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Tareas: {TareasGlobales.Count}");

                // 5. Fricciones
                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesGlobales = resFri.Data?.Fricciones?.Select(f => new FriccionModel
                {
                    FRI_ID = f.Fri_ID,
                    FRI_TIP = f.Fri_TIP,
                    FRI_DES = f.Fri_DES,
                    FRI_EST = f.Fri_EST,
                    FRI_IMP = f.Fri_IMP,
                    TAR_ID = f.Tar_ID,
                    FRI_FEC_CRE = f.Fri_FEC_CRE.DateTime
                }).ToList() ?? new();
                Console.WriteLine($"✅ Fricciones: {FriccionesGlobales.Count}");

                // 6. Prioridades
                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                PrioridadesList = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Prioridades: {PrioridadesList.Count}");

                // Asignar colores a tareas
                int i = 0;
                foreach (var t in TareasGlobales)
                {
                    if (!ColoresTareas.ContainsKey(t.TAR_ID))
                        ColoresTareas[t.TAR_ID] = PaletaColores[i++ % PaletaColores.Length];
                }

                ProcesosFiltrados = ProcesosGlobales;
                TareasFiltradas = TareasGlobales;
                FriccionesFiltradas = FriccionesGlobales;

                Console.WriteLine("========================================");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
            }
        }

        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            Console.WriteLine("\n🔍 APLICANDO FILTROS");
            Console.WriteLine($"📊 Total: {FriccionesGlobales.Count}");
            Console.WriteLine($"🎯 Activos: {filtros?.Count ?? 0}");

            if (filtros == null || !filtros.Any())
            {
                FriccionesFiltradas = FriccionesGlobales;
                StateHasChanged();
                return;
            }

            var resultado = FriccionesGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                Console.WriteLine($"🔧 {filtro.Tipo} → {filtro.Etiqueta}");

                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(f =>
                            f.FRI_TIP.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Tarea":
                        resultado = resultado.Where(f => f.TAR_ID == filtro.Id);
                        break;

                    case "Proceso":
                        var tareasIds = TareasGlobales
                            .Where(t => t.PROC_ID == filtro.Id)
                            .Select(t => t.TAR_ID)
                            .ToList();
                        resultado = resultado.Where(f =>
                            f.TAR_ID.HasValue && tareasIds.Contains(f.TAR_ID.Value));
                        break;

                    case "Proyecto":
                        var procesosIds = ProcesosGlobales
                            .Where(p => p.PRO_ID == filtro.Id)
                            .Select(p => p.PROC_ID)
                            .ToList();
                        var tIds = TareasGlobales
                            .Where(t => t.PROC_ID.HasValue && procesosIds.Contains(t.PROC_ID.Value))
                            .Select(t => t.TAR_ID)
                            .ToList();
                        resultado = resultado.Where(f =>
                            f.TAR_ID.HasValue && tIds.Contains(f.TAR_ID.Value));
                        break;

                    case "Area":
                        var proyIds = ProyectosList
                            .Where(p => p.ARE_ID == filtro.Id)
                            .Select(p => p.PRO_ID)
                            .ToList();
                        var procIds = ProcesosGlobales
                            .Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value))
                            .Select(p => p.PROC_ID)
                            .ToList();
                        var tarIds = TareasGlobales
                            .Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value))
                            .Select(t => t.TAR_ID)
                            .ToList();
                        resultado = resultado.Where(f =>
                            f.TAR_ID.HasValue && tarIds.Contains(f.TAR_ID.Value));
                        break;

                    case "Prioridad":
                        resultado = resultado.Where(f =>
                            f.FRI_IMP.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                }

                Console.WriteLine($"   → Quedan: {resultado.Count()}");
            }

            FriccionesFiltradas = resultado.ToList();
            Console.WriteLine($"📊 FINAL: {FriccionesFiltradas.Count}\n");
            StateHasChanged();
        }

        private void LimpiarFiltros()
        {
            FriccionesFiltradas = FriccionesGlobales;
            BusquedaNombre = "";
            StateHasChanged();
        }

        private void SeleccionarFriccion(FriccionModel fri)
        {
            FriccionSeleccionada = fri;
            StateHasChanged();
        }

        private void AbrirDetalleFriccion(FriccionModel fri)
        {
            FriccionSeleccionada = fri;
            Console.WriteLine($"🔍 Abriendo detalle: {fri.FRI_TIP}");
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            FriccionSeleccionada = null;
            StateHasChanged();
        }

        private void VerTareaPadre()
        {
            if (FriccionSeleccionada?.TAR_ID.HasValue == true)
            {
                Console.WriteLine($"🔗 Navegando a Tarea: {FriccionSeleccionada.TAR_ID.Value}");
                OnVerTarea.InvokeAsync(FriccionSeleccionada.TAR_ID.Value);
            }
            else
            {
                Console.WriteLine("⚠️ No hay tarea vinculada a esta fricción");
            }
        }

        private string ObtenerColorTarea(Guid? id) => (id.HasValue && ColoresTareas.ContainsKey(id.Value)) ? ColoresTareas[id.Value] : "#cbd5e1";
        private string ObtenerNombreTarea(Guid? id) => TareasGlobales.FirstOrDefault(t => t.TAR_ID == id)?.TAR_NOM ?? "Sin Tarea";
        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}
