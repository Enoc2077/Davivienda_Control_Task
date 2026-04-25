using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public partial class Proyectos : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        public List<ProyectosModel> ListaProyectos { get; set; } = new();
        public List<ProyectosModel> ProyectosFiltrados { get; set; } = new();
        public List<AreasModel> ListaAreas { get; set; } = new();

        public bool MostrarModalProcesos { get; set; } = false;
        public bool MostrarBitacoraProyecto { get; set; } = false;
        public string ModalActual { get; set; } = "";
        public string TextoBusqueda { get; set; } = "";
        public Guid? AreaIdFiltro { get; set; }

        public ProyectosModel? ProyectoSeleccionado { get; set; }
        public ProyectosModel? ProyectoFiltroAvance { get; set; }

        public DateTime FechaCalendario { get; set; } = DateTime.Today;
        public List<CalendarDay> DiasDelMes { get; set; } = new();
        public CalendarDay? DiaSeleccionado { get; set; }

        // 🔥 PUBLIC para que el .razor pueda acceder
        public string UserRole { get; set; } = "";
        public Guid? UserAreaId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await ObtenerDatosUsuario();
            await CargarDatos();
            GenerarCalendario();
            DiaSeleccionado = DiasDelMes.FirstOrDefault(d => d.Fecha.Date == DateTime.Today.Date);
        }

        private async Task ObtenerDatosUsuario()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                UserRole = user.FindFirst(ClaimTypes.Role)?.Value ?? "";

                var usuNumClaim = user.FindFirst("USU_NUM")?.Value ?? "";
                if (!string.IsNullOrEmpty(usuNumClaim))
                {
                    var resUsuarios = await Client.GetUsuarios.ExecuteAsync();
                    var usuData = resUsuarios.Data?.Usuarios.FirstOrDefault(u => u.Usu_NUM == usuNumClaim);
                    if (usuData != null)
                    {
                        UserAreaId = usuData.Are_ID;
                    }
                }
            }
        }

        public async Task CargarDatos()
        {
            try
            {
                await ObtenerDatosUsuario();
                ListaProyectos.Clear();
                ProyectosFiltrados.Clear();

                var resProy = await Client.GetProyectos.ExecuteAsync();
                var proyectosData = resProy.Data?.Proyectos;

                if (proyectosData != null)
                {
                    var todosProyectos = proyectosData.Select(p => new ProyectosModel
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
                    }).ToList();

                    ListaProyectos = FiltrarProyectosPorRol(todosProyectos)
                        .Where(p => p.PRO_EST != "FINALIZADO")
                        .ToList();
                }

                var resAreas = await Client.GetAreas.ExecuteAsync();
                ListaAreas = resAreas.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();

                Filtrar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private List<ProyectosModel> FiltrarProyectosPorRol(List<ProyectosModel> proyectos)
        {
            if (EsGerente(UserRole))
                return proyectos;

            if (UserAreaId.HasValue && UserAreaId != Guid.Empty)
                return proyectos.Where(p => p.ARE_ID == UserAreaId.Value).ToList();

            return new List<ProyectosModel>();
        }

        // 🔥 PUBLIC para que el .razor pueda usarlo en @if
        public bool EsGerente(string rol)
        {
            var rolesGerente = new[] { "Gerente", "Administrador", "Enoc", "Admin" };
            return rolesGerente.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        private bool EsLiderTecnico(string rol)
        {
            var rolesLider = new[] { "Líder Técnico", "LiderTecnico", "Lider", "Líder" };
            return rolesLider.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        public void AbrirModalNuevoProyecto()
        {
            ProyectoSeleccionado = new ProyectosModel
            {
                PRO_ID = Guid.Empty,
                PRO_NOM = "",
                PRO_EST = "NUEVO",
                PRO_FEC_INI = DateTimeOffset.Now,
                ARE_ID = EsLiderTecnico(UserRole) ? UserAreaId : null
            };
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
            GenerarCalendario();
        }

        public void AbrirBitacoraProyecto()
        {
            MostrarBitacoraProyecto = true;
        }

        public async Task CerrarBitacoraProyecto()
        {
            MostrarBitacoraProyecto = false;
            await CargarDatos();
            GenerarCalendario();
            StateHasChanged();
        }

        public void OnSearchChanged(ChangeEventArgs e)
        {
            TextoBusqueda = e.Value?.ToString() ?? "";
            Filtrar();
        }

        public void OnAreaFilterChanged(ChangeEventArgs e)
        {
            AreaIdFiltro = Guid.TryParse(e.Value?.ToString(), out Guid id) ? id : null;
            Filtrar();
        }

        private void Filtrar()
        {
            ProyectosFiltrados = ListaProyectos
                .Where(p =>
                    (string.IsNullOrEmpty(TextoBusqueda) ||
                     p.PRO_NOM.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)) &&
                    (!AreaIdFiltro.HasValue || p.ARE_ID == AreaIdFiltro))
                .ToList();
        }

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
            ProyectoFiltroAvance = ListaProyectos.FirstOrDefault(p =>
                p.PRO_FEC_INI.Date == dia.Fecha.Date ||
                (p.PRO_FEC_FIN.HasValue && p.PRO_FEC_FIN.Value.Date == dia.Fecha.Date));
            StateHasChanged();
        }

        public void VerDetalles(ProyectosModel proy)
        {
            if (ProyectoFiltroAvance?.PRO_ID == proy.PRO_ID)
                ProyectoFiltroAvance = null;
            else
                ProyectoFiltroAvance = proy;
            StateHasChanged();
        }

        public void MesAnterior()
        {
            FechaCalendario = FechaCalendario.AddMonths(-1);
            GenerarCalendario();
        }

        public void SiguienteMes()
        {
            FechaCalendario = FechaCalendario.AddMonths(1);
            GenerarCalendario();
        }

        public class CalendarDay
        {
            public DateTime Fecha { get; set; }
            public bool EsMesActual { get; set; }
            public bool EsHoy { get; set; }
            public bool EsInicioProyecto { get; set; }
            public bool EsFinProyecto { get; set; }
        }
    }
}