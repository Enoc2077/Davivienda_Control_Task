using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
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

        [Parameter] public List<TareaModel>? Tasks { get; set; }
        [Parameter] public List<PrioridadModel>? Prioridades { get; set; }

        public UsuarioModel? UsuarioActual { get; set; }
        public List<TareaModel> TareasFiltradas { get; set; } = new();
        public string PrioridadSeleccionada { get; set; } = "Todas";

        // Lógica de Calendario
        public DateTime FechaCalendario { get; set; } = DateTime.Today;
        public List<CalendarDayAsignacion> DiasDelMesAsignaciones { get; set; } = new();
        public bool MostrarModal { get; set; } = false;

        public string Initials => GetInitials(UsuarioActual?.USU_NOM);

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosDesdeBase();
            GenerarCalendarioMini();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await Task.Delay(250);
                    await JSRuntime.InvokeVoidAsync("setupHorizontalScroll", "scroll-container");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error de Interop: {ex.Message}");
                }
            }
        }

        private async Task CargarDatosDesdeBase()
        {
            try
            {
                var response = await Client.GetTareas.ExecuteAsync();
                if (response.Data?.Tareas != null)
                {
                    Tasks = response.Data.Tareas.Select(t => new TareaModel
                    {
                        TAR_ID = t.Tar_ID,
                        TAR_NOM = t.Tar_NOM,
                        TAR_DES = t.Tar_DES,
                        TAR_EST = t.Tar_EST,
                        PRI_ID = t.Pri_ID,
                        USU_ID = t.Usu_ID,
                        // Conversión explícita de DateTimeOffset a DateTime para evitar error CS1503
                        TAR_FEC_INI = t.Tar_FEC_INI.DateTime,
                        TAR_FEC_FIN = t.Tar_FEC_FIN?.DateTime
                    }).ToList();
                    TareasFiltradas = Tasks;
                }

                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                Prioridades = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList();

                UsuarioActual = new UsuarioModel { USU_NOM = "Usuario Davivienda" };
            }
            catch (Exception ex) { Console.WriteLine($"Error de conexión: {ex.Message}"); }
        }

        public void MesAnterior()
        {
            FechaCalendario = FechaCalendario.AddMonths(-1);
            GenerarCalendarioMini();
        }

        public void MesSiguiente()
        {
            FechaCalendario = FechaCalendario.AddMonths(1);
            GenerarCalendarioMini();
        }

        public void GenerarCalendarioMini()
        {
            DiasDelMesAsignaciones.Clear();
            var primeroMes = new DateTime(FechaCalendario.Year, FechaCalendario.Month, 1);
            int offset = (int)primeroMes.DayOfWeek;
            var fechaInicio = primeroMes.AddDays(-offset);

            for (int i = 0; i < 35; i++)
            {
                var fechaActual = fechaInicio.AddDays(i);
                DiasDelMesAsignaciones.Add(new CalendarDayAsignacion
                {
                    Fecha = fechaActual,
                    EsMesActual = fechaActual.Month == FechaCalendario.Month,
                    EsHoy = fechaActual.Date == DateTime.Today.Date,
                    TieneTareas = Tasks?.Any(t => t.TAR_FEC_INI.Date == fechaActual.Date) ?? false
                });
            }
        }

        public TareaModel? GetProximaTarea() => Tasks?
            .Where(t => t.TAR_FEC_FIN.HasValue && t.TAR_FEC_FIN >= DateTime.Now)
            .OrderBy(t => t.TAR_FEC_FIN).FirstOrDefault();

        private void FiltrarPorPrioridad(string nombrePrioridad)
        {
            PrioridadSeleccionada = nombrePrioridad;
            TareasFiltradas = nombrePrioridad == "Todas" ? Tasks ?? new() : Tasks?.Where(t => GetPrioridadNombre(t.PRI_ID) == nombrePrioridad).ToList() ?? new();
        }

        public string GetPrioridadNombre(Guid? priId) => Prioridades?.FirstOrDefault(p => p.PRI_ID == priId)?.PRI_NOM ?? "Baja";

        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "DB";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : $"{parts[0][0]}".ToUpper();
        }

        // CONTROL DEL MODAL
        private void AbrirModalCalendario()
        {
            MostrarModal = true;
            StateHasChanged();
        }

        private void CerrarModalCalendario()
        {
            MostrarModal = false;
            StateHasChanged();
        }

        public class CalendarDayAsignacion
        {
            public DateTime Fecha { get; set; }
            public bool EsMesActual { get; set; }
            public bool EsHoy { get; set; }
            public bool TieneTareas { get; set; }
        }
    }
}