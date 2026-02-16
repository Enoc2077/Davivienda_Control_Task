using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class BitacoraSolucion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        private List<SolucionesModel> SolucionesGlobales = new();
        private List<SolucionesModel> SolucionesFiltradas = new();
        private List<FriccionModel> FriccionesGlobales = new();
        private List<TareaModel> TareasGlobales = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<ProyectosModel> ProyectosList = new();

        private List<ProcesoModel> ProcesosFiltrados = new();
        private List<TareaModel> TareasFiltradas = new();

        private bool MostrarDetalle = false;
        private SolucionesModel? SolucionSeleccionada;
        private Dictionary<Guid, string> ColoresTareas = new();
        private string[] PaletaColores = { "#3b82f6", "#ef4444", "#10b981", "#f59e0b", "#8b5cf6", "#06b6d4" };

        protected override async Task OnInitializedAsync() => await CargarTodo();

        private async Task CargarTodo()
        {
            try
            {
                var resPro = await Client.GetProyectos.ExecuteAsync();
                ProyectosList = resPro.Data?.Proyectos?.Select(p => new ProyectosModel { PRO_ID = p.Pro_ID, PRO_NOM = p.Pro_NOM }).ToList() ?? new();

                var resPrc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resPrc.Data?.Procesos?.Select(p => new ProcesoModel { PROC_ID = p.Proc_ID, PROC_NOM = p.Proc_NOM, PRO_ID = p.Pro_ID }).ToList() ?? new();

                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasGlobales = resTar.Data?.Tareas?.Select(t => new TareaModel { TAR_ID = t.Tar_ID, TAR_NOM = t.Tar_NOM, PROC_ID = t.Proc_ID }).ToList() ?? new();

                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesGlobales = resFri.Data?.Fricciones?.Select(f => new FriccionModel { FRI_ID = f.Fri_ID, TAR_ID = f.Tar_ID }).ToList() ?? new();

                var resSol = await Client.GetSoluciones.ExecuteAsync();
                SolucionesGlobales = resSol.Data?.Soluciones?.Select(s => new SolucionesModel
                {
                    SOL_ID = s.Sol_ID,
                    SOL_NOM = s.Sol_NOM,
                    SOL_DES = s.Sol_DES,
                    SOL_EST = s.Sol_EST,
                    SOL_NIV_EFE = s.Sol_NIV_EFE,
                    FRI_ID = s.Fri_ID,
                    SOL_FEC_CRE = s.Sol_FEC_CRE.DateTime
                }).ToList() ?? new();

                int i = 0;
                foreach (var t in TareasGlobales) { ColoresTareas[t.TAR_ID] = PaletaColores[i++ % PaletaColores.Length]; }

                ProcesosFiltrados = ProcesosGlobales;
                TareasFiltradas = TareasGlobales;
                SolucionesFiltradas = SolucionesGlobales;
                StateHasChanged();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
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
                ActualizarSolucionesVisibles();
            }
            StateHasChanged();
        }

        private void OnProcesoChanged(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val)) ActualizarSolucionesVisibles();
            else
            {
                var prcId = Guid.Parse(val);
                TareasFiltradas = TareasGlobales.Where(t => t.PROC_ID == prcId).ToList();
                ActualizarSolucionesVisibles();
            }
            StateHasChanged();
        }

        private void OnTareaChanged(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val)) ActualizarSolucionesVisibles();
            else
            {
                var tarId = Guid.Parse(val);
                var friIds = FriccionesGlobales.Where(f => f.TAR_ID == tarId).Select(f => f.FRI_ID).ToList();
                SolucionesFiltradas = SolucionesGlobales.Where(s => s.FRI_ID.HasValue && friIds.Contains(s.FRI_ID.Value)).ToList();
            }
            StateHasChanged();
        }

        private void ActualizarSolucionesVisibles()
        {
            var tarIds = TareasFiltradas.Select(t => t.TAR_ID).ToList();
            var friIds = FriccionesGlobales.Where(f => f.TAR_ID.HasValue && tarIds.Contains(f.TAR_ID.Value)).Select(f => f.FRI_ID).ToList();
            SolucionesFiltradas = SolucionesGlobales.Where(s => s.FRI_ID.HasValue && friIds.Contains(s.FRI_ID.Value)).ToList();
        }

        private void AbrirDetalle(SolucionesModel sol) { SolucionSeleccionada = sol; MostrarDetalle = true; StateHasChanged(); }
        private void CerrarDetalle() { MostrarDetalle = false; SolucionSeleccionada = null; StateHasChanged(); }
        private string ObtenerColorTarea(Guid? friId)
        {
            var tarId = FriccionesGlobales.FirstOrDefault(f => f.FRI_ID == friId)?.TAR_ID;
            return (tarId.HasValue && ColoresTareas.ContainsKey(tarId.Value)) ? ColoresTareas[tarId.Value] : "#cbd5e1";
        }
        private string ObtenerNombreTareaPorFriccion(Guid? friId)
        {
            var tarId = FriccionesGlobales.FirstOrDefault(f => f.FRI_ID == friId)?.TAR_ID;
            return TareasGlobales.FirstOrDefault(t => t.TAR_ID == tarId)?.TAR_NOM ?? "Sin Tarea";
        }
    }
}