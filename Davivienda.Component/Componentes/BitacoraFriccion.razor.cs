using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class BitacoraFriccion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        private List<FriccionModel> FriccionesGlobales = new();
        private List<FriccionModel> FriccionesFiltradas = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<TareaModel> TareasGlobales = new();

        private List<ProcesoModel> ProcesosFiltrados = new();
        private List<TareaModel> TareasFiltradas = new();

        private bool MostrarDetalle = false;
        private FriccionModel? FriccionSeleccionada;
        private Dictionary<Guid, string> ColoresTareas = new();
        private string[] PaletaColores = { "#ef4444", "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6" };

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
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

                int i = 0;
                foreach (var t in TareasGlobales)
                {
                    if (!ColoresTareas.ContainsKey(t.TAR_ID))
                        ColoresTareas[t.TAR_ID] = PaletaColores[i++ % PaletaColores.Length];
                }

                ProcesosFiltrados = ProcesosGlobales;
                TareasFiltradas = TareasGlobales;
                FriccionesFiltradas = FriccionesGlobales;

                StateHasChanged();
            }
            catch (Exception ex) { Console.WriteLine($"Error en Bitácora: {ex.Message}"); }
        }

        private void OnProyectoChanged(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val))
            {
                ProcesosFiltrados = ProcesosGlobales;
                TareasFiltradas = TareasGlobales;
                FriccionesFiltradas = FriccionesGlobales;
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
                FriccionesFiltradas = FriccionesGlobales.Where(f => f.TAR_ID == tarId).ToList();
            }
            StateHasChanged();
        }

        private void AplicarFiltroFinal()
        {
            var idsTareas = TareasFiltradas.Select(t => t.TAR_ID).ToList();
            FriccionesFiltradas = FriccionesGlobales.Where(f => f.TAR_ID.HasValue && idsTareas.Contains(f.TAR_ID.Value)).ToList();
        }

        private void AbrirDetalle(FriccionModel fri) { FriccionSeleccionada = fri; MostrarDetalle = true; StateHasChanged(); }
        private void CerrarDetalle() { MostrarDetalle = false; FriccionSeleccionada = null; StateHasChanged(); }
        private string ObtenerColorTarea(Guid? id) => (id.HasValue && ColoresTareas.ContainsKey(id.Value)) ? ColoresTareas[id.Value] : "#cbd5e1";
        private string ObtenerNombreTarea(Guid? id) => TareasGlobales.FirstOrDefault(t => t.TAR_ID == id)?.TAR_NOM ?? "Sin Tarea";
    }
}