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
    public partial class Bitacoraproyecto : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        // 🔥 LISTAS PRINCIPALES
        private List<ProyectosModel> ProyectosGlobales = new();
        private List<ProyectosModel> ProyectosFiltrados = new();
        private List<AreasModel> AreasList = new();
        private List<ProcesoModel> ProcesosGlobales = new();

        // 🔥 FILTROS
        private string TextoBusqueda = "";
        private Guid? AreaFiltro = null;
        private string FechaFiltro = "todos";

        // 🔥 MODAL PROCESO
        public bool MostrarModalProceso { get; set; } = false;
        public Guid ProyectoSeleccionadoId { get; set; }
        public string ProyectoSeleccionadoNombre { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            await CargarTodo();
        }

        private async Task CargarTodo()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("📦 BITÁCORA PROYECTOS - CARGA INICIAL");
                Console.WriteLine("========================================");

                // 1. Proyectos
                var resProy = await Client.GetProyectos.ExecuteAsync();
                var todosProyectos = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    PRO_DES = p.Pro_DES,
                    PRO_EST = p.Pro_EST,
                    ARE_ID = p.Are_ID,
                    PRO_FEC_INI = p.Pro_FEC_INI,
                    PRO_FEC_FIN = p.Pro_FEC_FIN,
                    PRO_FEC_CRE = p.Pro_FEC_CRE,
                    PRO_FEC_MOD = p.Pro_FEC_MOD
                }).ToList() ?? new();

                Console.WriteLine($"✅ Proyectos totales: {todosProyectos.Count}");

                // 🔥 FILTRAR SOLO PROYECTOS FINALIZADOS
                ProyectosGlobales = todosProyectos
                    .Where(p => p.PRO_EST == "FINALIZADO")
                    .OrderByDescending(p => p.PRO_FEC_FIN ?? p.PRO_FEC_CRE)
                    .ToList();

                Console.WriteLine($"✅ Proyectos finalizados: {ProyectosGlobales.Count}");

                // 2. Áreas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();
                Console.WriteLine($"✅ Áreas: {AreasList.Count}");

                // 3. Procesos
                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resProc.Data?.Procesos.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PROC_EST = p.Proc_EST,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();
                Console.WriteLine($"✅ Procesos: {ProcesosGlobales.Count}");

                // Aplicar filtros iniciales
                AplicarFiltros();

                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
            }
        }

        private void OnBusquedaChanged(ChangeEventArgs e)
        {
            TextoBusqueda = e.Value?.ToString() ?? "";
            AplicarFiltros();
        }

        private void OnAreaChanged(ChangeEventArgs e)
        {
            AreaFiltro = Guid.TryParse(e.Value?.ToString(), out Guid id) ? id : null;
            AplicarFiltros();
        }

        private void OnFechaChanged(ChangeEventArgs e)
        {
            FechaFiltro = e.Value?.ToString() ?? "todos";
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            Console.WriteLine("\n🔍 APLICANDO FILTROS");
            Console.WriteLine($"📊 Total: {ProyectosGlobales.Count}");

            var resultado = ProyectosGlobales.AsEnumerable();

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                resultado = resultado.Where(p =>
                    p.PRO_NOM.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"🔧 Búsqueda: {TextoBusqueda} → Quedan: {resultado.Count()}");
            }

            // Filtro por área
            if (AreaFiltro.HasValue)
            {
                resultado = resultado.Where(p => p.ARE_ID == AreaFiltro.Value);
                Console.WriteLine($"🔧 Área: {AreaFiltro} → Quedan: {resultado.Count()}");
            }

            // Filtro por fecha
            if (FechaFiltro != "todos")
            {
                var hoy = DateTimeOffset.Now;
                resultado = FechaFiltro switch
                {
                    "mes" => resultado.Where(p =>
                        p.PRO_FEC_FIN.HasValue &&
                        p.PRO_FEC_FIN.Value.Year == hoy.Year &&
                        p.PRO_FEC_FIN.Value.Month == hoy.Month),
                    "trimestre" => resultado.Where(p =>
                        p.PRO_FEC_FIN.HasValue &&
                        p.PRO_FEC_FIN.Value >= hoy.AddMonths(-3)),
                    "año" => resultado.Where(p =>
                        p.PRO_FEC_FIN.HasValue &&
                        p.PRO_FEC_FIN.Value.Year == hoy.Year),
                    _ => resultado
                };
                Console.WriteLine($"🔧 Fecha: {FechaFiltro} → Quedan: {resultado.Count()}");
            }

            ProyectosFiltrados = resultado
                .OrderByDescending(p => p.PRO_FEC_FIN ?? p.PRO_FEC_CRE)
                .ToList();

            Console.WriteLine($"📊 FINAL: {ProyectosFiltrados.Count}\n");
            StateHasChanged();
        }

        private void AbrirProcesosDelProyecto(ProyectosModel proyecto)
        {
            ProyectoSeleccionadoId = proyecto.PRO_ID;
            ProyectoSeleccionadoNombre = proyecto.PRO_NOM;
            MostrarModalProceso = true;

            Console.WriteLine($"🔍 Abriendo Procesos del proyecto: {proyecto.PRO_NOM}");
            StateHasChanged();
        }

        private async Task CerrarModalProceso()
        {
            MostrarModalProceso = false;
            ProyectoSeleccionadoId = Guid.Empty;
            ProyectoSeleccionadoNombre = "";

            // 🔥 RECARGAR DATOS PARA VER SI EL PROYECTO SE REACTIVÓ
            await CargarTodo();

            Console.WriteLine("✅ Modal de Proceso cerrado - Datos recargados");
            StateHasChanged();
        }

        private string ObtenerNombreArea(Guid? areaId)
        {
            if (!areaId.HasValue) return "Sin área";
            var area = AreasList.FirstOrDefault(a => a.ARE_ID == areaId.Value);
            return area?.ARE_NOM ?? "Área no encontrada";
        }

        private int ObtenerCantidadProcesos(Guid proyectoId)
        {
            return ProcesosGlobales.Count(p => p.PRO_ID == proyectoId);
        }

        private async Task Regresar()
        {
            await OnClose.InvokeAsync();
        }
    }
}
