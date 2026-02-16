using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Asignaciones
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;

        public List<TareaModel>? Tasks { get; set; } = new();
        public List<PrioridadModel>? Prioridades { get; set; }
        public List<TareaModel> TareasFiltradas { get; set; } = new();
        public List<ProyectosModel> ProyectosRelacionados { get; set; } = new();

        public UsuarioModel? UsuarioActual { get; set; }
        public string PrioridadSeleccionada { get; set; } = "Todas";
        public string UserRole { get; set; } = "Empleado";

        public int TareasCompletadas { get; set; }
        public int TareasPendientes { get; set; }
        public int TareasBloqueadas { get; set; }
        public int TotalProyectos { get; set; }
        public int TotalProcesos { get; set; }



        public DateTime FechaCalendario { get; set; } = DateTime.Today;
        public List<CalendarDayAsignacion> DiasDelMesAsignaciones { get; set; } = new();
        public bool MostrarModal { get; set; } = false;
        public string ModalActual { get; set; } = "";
        public TareaModel? TareaSeleccionada { get; set; }
        public string Initials => !string.IsNullOrEmpty(UsuarioActual?.USU_NOM) ? string.Join("", UsuarioActual.USU_NOM.Split(' ').Select(x => x[0])).ToUpper() : "UD";

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosDesdeBase();
            GenerarCalendarioMini();
        }

        private async Task CargarDatosDesdeBase()
        {
            // Dentro de CargarDatosDesdeBase(), actualiza estas líneas:
            TareasCompletadas = Tasks.Count(t => t.TAR_EST == "Completado");
            // Agrupamos Pendientes y En Progreso para el color azul
            TareasPendientes = Tasks.Count(t => t.TAR_EST == "Pendiente" || t.TAR_EST == "En Progreso");
            TareasBloqueadas = Tasks.Count(t => t.TAR_EST == "Bloqueado");
            try
            {
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                // Determinamos el Rol [cite: 2026-02-10]
                UserRole = user.IsInRole("Administrador") ? "Administrador" : "Empleado";

                // Obtenemos al usuario de la base para tener su ID y Área
                var resUser = await Client.GetUsuarioByEmail.ExecuteAsync(user.Identity?.Name ?? "");
                if (resUser.Data?.UsuarioByEmail != null)
                {
                    UsuarioActual = new UsuarioModel
                    {
                        USU_ID = resUser.Data.UsuarioByEmail.Usu_ID,
                        USU_NOM = resUser.Data.UsuarioByEmail.Usu_NOM,
                        ARE_ID = resUser.Data.UsuarioByEmail.Are_ID
                    };
                }

                var response = await Client.GetTareas.ExecuteAsync();
                var allTasks = response.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_DES = t.Tar_DES,
                    TAR_EST = t.Tar_EST,
                    PRI_ID = t.Pri_ID,
                    USU_ID = t.Usu_ID,
                    TAR_FEC_INI = t.Tar_FEC_INI.DateTime,
                    TAR_FEC_FIN = t.Tar_FEC_FIN?.DateTime
                }).ToList() ?? new();

                // LÓGICA DE FILTRADO REPARADA:
                // Si es Admin ve todo. Si es Empleado ve lo suyo. Si no hay ID de usuario (Invitado), ve todo para no romper la vista.
                if (UserRole == "Administrador" || UsuarioActual?.USU_ID == null)
                {
                    Tasks = allTasks;
                }
                else
                {
                    Tasks = allTasks.Where(t => t.USU_ID == UsuarioActual.USU_ID).ToList();
                }

                TareasFiltradas = Tasks;

                // Estadísticas
                TareasCompletadas = Tasks.Count(t => t.TAR_EST == "Completado");
                TareasPendientes = Tasks.Count(t => t.TAR_EST == "Pendiente" || t.TAR_EST == "En Progreso");
                TareasBloqueadas = Tasks.Count(t => t.TAR_EST == "Bloqueado");

                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosRelacionados = resProy.Data?.Proyectos.Select(p => new ProyectosModel { PRO_ID = p.Pro_ID, PRO_NOM = p.Pro_NOM }).ToList() ?? new();
                TotalProyectos = ProyectosRelacionados.Count;

                var resProc = await Client.GetProcesos.ExecuteAsync();
                TotalProcesos = resProc.Data?.Procesos.Count() ?? 0;

                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                Prioridades = resPrio.Data?.Prioridades.Select(p => new PrioridadModel { PRI_ID = p.Pri_ID, PRI_NOM = p.Pri_NOM }).ToList();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        private double GetPct(int val) => (Tasks?.Count > 0) ? (val * 100.0 / Tasks.Count) : 0;
        public void MesAnterior() { FechaCalendario = FechaCalendario.AddMonths(-1); GenerarCalendarioMini(); }
        public void MesSiguiente() { FechaCalendario = FechaCalendario.AddMonths(1); GenerarCalendarioMini(); }

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

        public TareaModel? GetProximaTarea() => Tasks?.Where(t => t.TAR_FEC_FIN >= DateTime.Now).OrderBy(t => t.TAR_FEC_FIN).FirstOrDefault();
        private void FiltrarPorPrioridad(string np) { PrioridadSeleccionada = np; TareasFiltradas = np == "Todas" ? Tasks! : Tasks!.Where(t => GetPrioridadNombre(t.PRI_ID) == np).ToList(); }
        public string GetPrioridadNombre(Guid? id) => Prioridades?.FirstOrDefault(p => p.PRI_ID == id)?.PRI_NOM ?? "Baja";
        private void AbrirModalCalendario() { ModalActual = "CALENDARIO"; MostrarModal = true; }
        private void AbrirDetalleTarea(TareaModel t) { TareaSeleccionada = t; ModalActual = "DETALLE"; MostrarModal = true; }
        private void CerrarModal() { MostrarModal = false; ModalActual = ""; TareaSeleccionada = null; }

        public class CalendarDayAsignacion { public DateTime Fecha { get; set; } public bool EsMesActual { get; set; } public bool EsHoy { get; set; } public bool TieneTareas { get; set; } }
    }
}