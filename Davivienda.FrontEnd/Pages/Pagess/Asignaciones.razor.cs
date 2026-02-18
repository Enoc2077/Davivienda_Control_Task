using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Asignaciones
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;

        public List<TareaModel>? Tasks { get; set; } = new();
        public List<TareaModel> TareasFiltradas { get; set; } = new();
        public List<PrioridadModel>? Prioridades { get; set; }
        public List<ProyectosModel> ProyectosRelacionados { get; set; } = new();
        public UsuarioModel? UsuarioActual { get; set; }

        public string PrioridadSeleccionada { get; set; } = "Todas";
        public string UserRole { get; set; } = "";
        public DateTime FechaCalendario { get; set; } = DateTime.Today;
        public List<CalendarDayAsignacion> DiasDelMesAsignaciones { get; set; } = new();
        public bool MostrarModal { get; set; }
        public string ModalActual { get; set; } = "";
        public TareaModel? TareaSeleccionada { get; set; }

        public int TotalTareasCount { get; set; }
        public int TotalProyectos { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosDesdeBase();
            GenerarCalendarioMini();
        }

        private async Task CargarDatosDesdeBase()
        {
            try
            {
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                // Obtenemos el rol desde los Claims
                UserRole = user.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                           ?? user.Claims.FirstOrDefault(c => c.Type == "role")?.Value
                           ?? "Empleado";

                // 1. Cargar Usuario Actual
                var resUser = await Client.GetUsuarioByEmail.ExecuteAsync(user.Identity?.Name ?? "");
                if (resUser.Data?.UsuarioByEmail != null)
                {
                    UsuarioActual = new UsuarioModel
                    {
                        USU_ID = resUser.Data.UsuarioByEmail.Usu_ID,
                        USU_NOM = resUser.Data.UsuarioByEmail.Usu_NOM
                    };
                }

                // 2. Cargar Tareas Globales
                var response = await Client.GetTareas.ExecuteAsync();
                var allTasks = response.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_DES = t.Tar_DES,
                    TAR_EST = t.Tar_EST,
                    PRI_ID = t.Pri_ID,
                    USU_ID = t.Usu_ID,
                    TAR_FEC_INI = t.Tar_FEC_INI,
                    TAR_FEC_FIN = t.Tar_FEC_FIN
                }).ToList() ?? new();

                // 3. SEGURIDAD POR ROL: Filtrar acceso
                var rolesConAccesoTotal = new[] { "Gerente", "Líder Técnico", "Administrador" };

                if (rolesConAccesoTotal.Contains(UserRole))
                {
                    Tasks = allTasks; // Acceso total
                }
                else
                {
                    // Acceso restringido: Solo sus propias tareas
                    Tasks = allTasks.Where(t => t.USU_ID == UsuarioActual?.USU_ID).ToList();
                }

                TareasFiltradas = Tasks;
                TotalTareasCount = Tasks.Count;

                // 4. Cargar Proyectos
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosRelacionados = resProy.Data?.Proyectos.Select(p => new ProyectosModel { PRO_ID = p.Pro_ID, PRO_NOM = p.Pro_NOM }).ToList() ?? new();
                TotalProyectos = ProyectosRelacionados.Count;

                // 5. Cargar Prioridades
                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                Prioridades = resPrio.Data?.Prioridades.Select(p => new PrioridadModel { PRI_ID = p.Pri_ID, PRI_NOM = p.Pri_NOM }).ToList();
            }
            catch (Exception ex) { Console.WriteLine($"Error Dashboard: {ex.Message}"); }
        }

        // Lógica de Filtrado (Se usa desde las gráficas también)
        private void FiltrarPorPrioridad(string prio)
        {
            PrioridadSeleccionada = prio;
            TareasFiltradas = prio == "Todas" ? Tasks! : Tasks!.Where(t => GetPrioridadNombre(t.PRI_ID) == prio).ToList();
        }

        private void FiltrarPorEstadoGrafica(string estado)
        {
            if (estado == "Todas") TareasFiltradas = Tasks!;
            else TareasFiltradas = Tasks!.Where(t => t.TAR_EST == estado).ToList();
        }

        public string GetPrioridadNombre(Guid? id) => Prioridades?.FirstOrDefault(p => p.PRI_ID == id)?.PRI_NOM ?? "Baja";

        public void GenerarCalendarioMini()
        {
            DiasDelMesAsignaciones.Clear();
            var primeroMes = new DateTime(FechaCalendario.Year, FechaCalendario.Month, 1);
            int offset = (int)primeroMes.DayOfWeek;
            var fechaInicio = primeroMes.AddDays(-offset);
            for (int i = 0; i < 35; i++)
            {
                var f = fechaInicio.AddDays(i);
                DiasDelMesAsignaciones.Add(new CalendarDayAsignacion
                {
                    Fecha = f,
                    EsMesActual = f.Month == FechaCalendario.Month,
                    EsHoy = f.Date == DateTime.Today,
                    TieneTareas = Tasks?.Any(t => t.TAR_FEC_INI.Date == f.Date) ?? false
                });
            }
        }

        public TareaModel? GetProximaTarea() => Tasks?.Where(t => t.TAR_FEC_FIN >= DateTimeOffset.Now).OrderBy(t => t.TAR_FEC_FIN).FirstOrDefault();
        private void AbrirModalCalendario() { ModalActual = "CALENDARIO"; MostrarModal = true; }
        private void AbrirDetalleTarea(TareaModel t) { TareaSeleccionada = t; ModalActual = "DETALLE"; MostrarModal = true; }
        private void CerrarModal() { MostrarModal = false; ModalActual = ""; TareaSeleccionada = null; }
        public void MesAnterior() { FechaCalendario = FechaCalendario.AddMonths(-1); GenerarCalendarioMini(); }
        public void MesSiguiente() { FechaCalendario = FechaCalendario.AddMonths(1); GenerarCalendarioMini(); }

        public class CalendarDayAsignacion { public DateTime Fecha { get; set; } public bool EsMesActual { get; set; } public bool EsHoy { get; set; } public bool TieneTareas { get; set; } }
    }
}