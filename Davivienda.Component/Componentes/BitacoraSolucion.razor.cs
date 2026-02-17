using Microsoft.AspNetCore.Components;
using Davivienda.Models.Modelos;
using Davivienda.GraphQL.SDK;
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

        // Datos Globales
        private List<SolucionesModel> SolucionesGlobales = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<TareaModel> TareasGlobales = new();

        // Datos Filtrados (UI)
        private List<SolucionesModel> SolucionesFiltradas = new();
        private List<ProcesoModel> ProcesosFiltrados = new();
        private List<TareaModel> TareasFiltradas = new();

        // Estado de Filtros
        public string BusquedaNombre { get; set; } = "";
        public SolucionesModel? SolucionSeleccionada { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosList = resProy.Data?.Proyectos?.Select(p => new ProyectosModel { PRO_ID = p.Pro_ID, PRO_NOM = p.Pro_NOM }).ToList() ?? new();

                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resProc.Data?.Procesos?.Select(p => new ProcesoModel { PROC_ID = p.Proc_ID, PROC_NOM = p.Proc_NOM, PRO_ID = p.Pro_ID }).ToList() ?? new();

                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasGlobales = resTar.Data?.Tareas?.Select(t => new TareaModel { TAR_ID = t.Tar_ID, TAR_NOM = t.Tar_NOM, PROC_ID = t.Proc_ID }).ToList() ?? new();

                var resSol = await Client.GetSoluciones.ExecuteAsync();
                SolucionesGlobales = resSol.Data?.Soluciones?.Select(s => new SolucionesModel
                {
                    SOL_ID = s.Sol_ID,
                    SOL_NOM = s.Sol_NOM,
                    SOL_DES = s.Sol_DES,
                    SOL_TIE_RES = s.Sol_TIE_RES,
                    SOL_NIV_EFE = s.Sol_NIV_EFE,
                    SOL_EST = s.Sol_EST,
                    SOL_FEC_CRE = s.Sol_FEC_CRE,
                    FRI_ID = s.Fri_ID,
                    USU_ID = s.Usu_ID
                }).ToList() ?? new();

                // Inicializar listas visibles
                ProcesosFiltrados = ProcesosGlobales;
                TareasFiltradas = TareasGlobales;
                SolucionesFiltradas = SolucionesGlobales;

                StateHasChanged();
            }
            catch (Exception ex) { Console.WriteLine($"Error de carga: {ex.Message}"); }
        }

        private void OnProyectoChanged(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val))
            {
                ProcesosFiltrados = ProcesosGlobales;
                TareasFiltradas = TareasGlobales;
                SolucionesFiltradas = SolucionesGlobales;
            }
            else
            {
                var proId = Guid.Parse(val);
                ProcesosFiltrados = ProcesosGlobales.Where(p => p.PRO_ID == proId).ToList();
                var procIds = ProcesosFiltrados.Select(p => p.PROC_ID).ToList();
                TareasFiltradas = TareasGlobales.Where(t => procIds.Contains(t.PROC_ID ?? Guid.Empty)).ToList();
                AplicarFiltroFinal();
            }
            StateHasChanged();
        }

        private void OnProcesoChanged(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val)) AplicarFiltroFinal();
            else
            {
                var prcId = Guid.Parse(val);
                TareasFiltradas = TareasGlobales.Where(t => t.PROC_ID == prcId).ToList();
                AplicarFiltroFinal();
            }
            StateHasChanged();
        }

        private void OnTareaChanged(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val)) AplicarFiltroFinal();
            else
            {
                var tarId = Guid.Parse(val);
                SolucionesFiltradas = SolucionesGlobales.Where(s => s.FRI_ID == tarId).ToList();
            }
            StateHasChanged();
        }

        private void AplicarFiltroFinal()
        {
            var idsTareas = TareasFiltradas.Select(t => t.TAR_ID).ToList();
            SolucionesFiltradas = SolucionesGlobales.Where(s => s.FRI_ID.HasValue && idsTareas.Contains(s.FRI_ID.Value)).ToList();

            if (!string.IsNullOrEmpty(BusquedaNombre))
                SolucionesFiltradas = SolucionesFiltradas.Where(s => s.SOL_NOM.Contains(BusquedaNombre, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void LimpiarFiltros()
        {
            BusquedaNombre = "";
            ProcesosFiltrados = ProcesosGlobales;
            TareasFiltradas = TareasGlobales;
            SolucionesFiltradas = SolucionesGlobales;
            StateHasChanged();
        }

        private void SeleccionarSolucion(SolucionesModel sol) { SolucionSeleccionada = sol; StateHasChanged(); }
        private string ObtenerNombreFriccion(Guid? id) => TareasGlobales.FirstOrDefault(t => t.TAR_ID == id)?.TAR_NOM ?? "Sin Tarea";
        private string ObtenerNombreUsuario(Guid? id) => "Usuario Técnico";
        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}