using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public partial class Proyectos : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        public List<ProyectosModel> ListaProyectos { get; set; } = new();
        public List<ProyectosModel> ProyectosFiltrados { get; set; } = new();
        public List<AreasModel> ListaAreas { get; set; } = new();

        public bool MostrarModalProcesos { get; set; } = false;
        public bool EsNuevoProyecto { get; set; } = false;
        public string ModalActual { get; set; } = ""; // "NUEVO", "EDITAR", "PROCESOS"
        public string TextoBusqueda { get; set; } = "";
        public Guid? AreaIdFiltro { get; set; }

        public ProyectosModel? ProyectoSeleccionado { get; set; }
        public ProyectosModel? ProyectoFiltroAvance { get; set; }

        public DateTime FechaCalendario { get; set; } = DateTime.Today;
        public List<CalendarDay> DiasDelMes { get; set; } = new();
        public CalendarDay? DiaSeleccionado { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
            GenerarCalendario();
            DiaSeleccionado = DiasDelMes.FirstOrDefault(d => d.Fecha.Date == DateTime.Today.Date);
        }

        public async Task CargarDatos()
        {
            try
            {
                var resProy = await Client.GetProyectos.ExecuteAsync();
                var proyectosData = resProy.Data?.Proyectos;
                if (proyectosData != null && proyectosData.Any())
                {
                    ListaProyectos = proyectosData.Select(p => new ProyectosModel
                    {
                        PRO_ID = p.Pro_ID,
                        PRO_NOM = p.Pro_NOM,
                        PRO_EST = p.Pro_EST,
                        ARE_ID = p.Are_ID,
                        PRO_FEC_INI = p.Pro_FEC_INI,
                        PRO_FEC_FIN = p.Pro_FEC_FIN
                    }).ToList();
                }
                else { CargarDatosDePrueba(); }

                var resAreas = await Client.GetAreas.ExecuteAsync();
                ListaAreas = resAreas.Data?.Areas.Select(a => new AreasModel { ARE_ID = a.Are_ID, ARE_NOM = a.Are_NOM }).ToList() ?? new();
                Filtrar();
            }
            catch { CargarDatosDePrueba(); }
            StateHasChanged();
        }

        public void AbrirModalNuevoProyecto()
        {
            ProyectoSeleccionado = new ProyectosModel { PRO_ID = Guid.Empty, PRO_NOM = "", PRO_EST = "NUEVO", PRO_FEC_INI = DateTimeOffset.Now };
            ModalActual = "NUEVO";
            MostrarModalProcesos = true;
        }

        public void AbrirModalEditar(ProyectosModel proy)
        {
            ProyectoSeleccionado = proy;
            ModalActual = "EDITAR";
            MostrarModalProcesos = true;
        }

        public void AbrirModalProcesos(ProyectosModel proy)
        {
            ProyectoSeleccionado = proy;
            ModalActual = "PROCESOS";
            MostrarModalProcesos = true;
        }

        public async Task CerrarModalProceso()
        {
            MostrarModalProcesos = false;
            ModalActual = "";
            await CargarDatos();
        }

        public void OnSearchChanged(ChangeEventArgs e) { TextoBusqueda = e.Value?.ToString() ?? ""; Filtrar(); }
        public void OnAreaFilterChanged(ChangeEventArgs e) { AreaIdFiltro = Guid.TryParse(e.Value?.ToString(), out Guid id) ? id : null; Filtrar(); }
        private void Filtrar() => ProyectosFiltrados = ListaProyectos.Where(p => (string.IsNullOrEmpty(TextoBusqueda) || p.PRO_NOM.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)) && (!AreaIdFiltro.HasValue || p.ARE_ID == AreaIdFiltro)).ToList();

        public void GenerarCalendario()
        {
            DiasDelMes.Clear();
            var primeroMes = new DateTime(FechaCalendario.Year, FechaCalendario.Month, 1);
            int offset = (int)primeroMes.DayOfWeek;
            var fechaActual = primeroMes.AddDays(-offset);
            for (int i = 0; i < 42; i++)
            {
                DiasDelMes.Add(new CalendarDay
                {
                    Fecha = fechaActual,
                    EsMesActual = fechaActual.Month == FechaCalendario.Month,
                    EsHoy = fechaActual.Date == DateTime.Today.Date,
                    EsInicioProyecto = ListaProyectos.Any(p => p.PRO_FEC_INI.Date == fechaActual.Date),
                    EsFinProyecto = ListaProyectos.Any(p => p.PRO_FEC_FIN.HasValue && p.PRO_FEC_FIN.Value.Date == fechaActual.Date)
                });
                fechaActual = fechaActual.AddDays(1);
            }
        }

        public void SeleccionarDiaCalendario(CalendarDay dia)
        {
            DiaSeleccionado = dia;
            ProyectoFiltroAvance = ListaProyectos.FirstOrDefault(p => p.PRO_FEC_INI.Date == dia.Fecha.Date || (p.PRO_FEC_FIN.HasValue && p.PRO_FEC_FIN.Value.Date == dia.Fecha.Date));
            StateHasChanged();
        }

        public void VerDetalles(ProyectosModel proy)
        {
            if (ProyectoFiltroAvance?.PRO_ID == proy.PRO_ID) ProyectoFiltroAvance = null;
            else ProyectoFiltroAvance = proy;
            StateHasChanged();
        }

        public void MesAnterior() { FechaCalendario = FechaCalendario.AddMonths(-1); GenerarCalendario(); }
        public void SiguienteMes() { FechaCalendario = FechaCalendario.AddMonths(1); GenerarCalendario(); }

        public class CalendarDay { public DateTime Fecha { get; set; } public bool EsMesActual { get; set; } public bool EsHoy { get; set; } public bool EsInicioProyecto { get; set; } public bool EsFinProyecto { get; set; } }
        private void CargarDatosDePrueba() { /* Datos de prueba igual que antes */ }
    }
}