using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class Calendario : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        private DateTime CurrentMonth { get; set; } = DateTime.Today;
        private DateTime SelectedDate { get; set; } = DateTime.Today;
        private DateTime CurrentWeekStart { get; set; }
        private List<TareaModel>? ListaTareas { get; set; }
        private List<EventoCalendario> EventosCalendario { get; set; } = new();
        private TareaModel? TareaEnFoco;
        private int CountAlta, CountMedia, CountBaja;

        protected override async Task OnInitializedAsync()
        {
            CurrentWeekStart = GetWeekStart(DateTime.Today);
            await CargarTareasDesdeDB();
        }

        private async Task CargarTareasDesdeDB()
        {
            try
            {
                var response = await Client.GetTareas.ExecuteAsync();
                if (response.Data?.Tareas != null)
                {
                    var rnd = new Random();
                    ListaTareas = response.Data.Tareas.Select(t => {
                        var fIni = t.Tar_FEC_INI.DateTime;
                        var fFin = t.Tar_FEC_FIN?.DateTime;

                        // Si no tiene hora, asignar aleatoria
                        if (fIni.Hour == 0 && fIni.Minute == 0)
                        {
                            int horaRnd = rnd.Next(8, 17);
                            int minRnd = rnd.Next(0, 4) * 15;
                            fIni = new DateTime(fIni.Year, fIni.Month, fIni.Day, horaRnd, minRnd, 0);
                        }

                        if (!fFin.HasValue || (fFin.Value.Hour == 0 && fFin.Value.Minute == 0))
                        {
                            if (fFin.HasValue)
                                fFin = new DateTime(fFin.Value.Year, fFin.Value.Month, fFin.Value.Day, fIni.Hour, fIni.Minute, 0);
                            else
                                fFin = fIni.AddHours(1);
                        }

                        return new TareaModel
                        {
                            TAR_ID = t.Tar_ID,
                            TAR_NOM = t.Tar_NOM,
                            TAR_DES = t.Tar_DES,
                            TAR_EST = t.Tar_EST,
                            TAR_FEC_INI = fIni,
                            TAR_FEC_FIN = fFin,
                            PRI_ID = t.Pri_ID
                        };
                    }).OrderBy(t => t.TAR_FEC_INI).ToList();

                    // 🔥 GENERAR EVENTOS (2 por tarea: INICIO y FIN)
                    GenerarEventos();
                    ActualizarEstadisticas();
                    TareaEnFoco = ListaTareas.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // 🔥 CREAR DOS EVENTOS POR TAREA
        private void GenerarEventos()
        {
            EventosCalendario.Clear();
            if (ListaTareas == null) return;

            foreach (var tarea in ListaTareas)
            {
                // Evento de INICIO (Morado)
                EventosCalendario.Add(new EventoCalendario
                {
                    TareaId = tarea.TAR_ID,
                    Nombre = tarea.TAR_NOM,
                    Fecha = tarea.TAR_FEC_INI.DateTime,
                    Hora = tarea.TAR_FEC_INI.ToString("HH:mm"),
                    TipoEvento = "INICIO",
                    TareaRef = tarea
                });

                // Evento de FIN (Rojo) - solo si tiene fecha fin
                if (tarea.TAR_FEC_FIN.HasValue)
                {
                    EventosCalendario.Add(new EventoCalendario
                    {
                        TareaId = tarea.TAR_ID,
                        Nombre = tarea.TAR_NOM,
                        Fecha = tarea.TAR_FEC_FIN.Value.DateTime,
                        Hora = tarea.TAR_FEC_FIN.Value.ToString("HH:mm"),
                        TipoEvento = "FIN",
                        TareaRef = tarea
                    });
                }
            }
        }

        private string GetEventoPosicion(DateTime fecha, string tipo)
        {
            double inicioDecimal = fecha.Hour + (fecha.Minute / 60.0);
            double duracion = 2.0; // 🔥 2 horas para que sea más largo
            double top = inicioDecimal * 60;
            double height = duracion * 60;

            return $"top: {top}px; height: {height}px;";
        }

        private string GetEventoClase(string tipo)
        {
            return tipo == "FIN" ? "event-fin" : "event-inicio";
        }

        private void ActualizarEstadisticas()
        {
            if (ListaTareas == null) return;
            var activas = ListaTareas.Where(t => t.TAR_EST != "Completado").ToList();
            CountAlta = activas.Count(t => GetPrioridadCategoria(t.PRI_ID) == "Alta");
            CountMedia = activas.Count(t => GetPrioridadCategoria(t.PRI_ID) == "Media");
            CountBaja = activas.Count(t => GetPrioridadCategoria(t.PRI_ID) == "Baja");
        }

        private void EnfocarTarea(TareaModel tarea)
        {
            TareaEnFoco = tarea;
            StateHasChanged();
        }

        private string GetPrioridadCategoria(Guid? priId)
        {
            if (priId == null) return "Baja";
            var idStr = priId.ToString().ToLower();
            if (idStr.Contains("1") || idStr.Contains("alta")) return "Alta";
            if (idStr.Contains("2") || idStr.Contains("media")) return "Media";
            return "Baja";
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private List<CalendarDay> GetMonthDays()
        {
            var days = new List<CalendarDay>();
            var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int offset = ((int)firstDay.DayOfWeek - 1 + 7) % 7;
            var start = firstDay.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                var d = start.AddDays(i);
                bool inicia = ListaTareas?.Any(t => t.TAR_FEC_INI.Date == d.Date) ?? false;
                bool finaliza = ListaTareas?.Any(t => t.TAR_FEC_FIN.HasValue && t.TAR_FEC_FIN.Value.Date == d.Date) ?? false;

                days.Add(new CalendarDay
                {
                    Date = d,
                    Day = d.Day,
                    IsCurrentMonth = d.Month == CurrentMonth.Month,
                    IsToday = d.Date == DateTime.Today,
                    IsSelected = d.Date == SelectedDate.Date,
                    TieneTareaPorIniciar = inicia,
                    TieneTareaPorFinalizar = finaliza
                });
            }
            return days;
        }

        private List<DateTime> GetWeekDays() => Enumerable.Range(0, 7).Select(i => CurrentWeekStart.AddDays(i)).ToList();
        private string GetWeekRange() => $"{CurrentWeekStart:MMMM d} - {CurrentWeekStart.AddDays(6):d, yyyy}";
        private void PreviousMonth() => CurrentMonth = CurrentMonth.AddMonths(-1);
        private void NextMonth() => CurrentMonth = CurrentMonth.AddMonths(1);
        private void PreviousWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        private void NextWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(7);

        private void SelectDay(CalendarDay day)
        {
            SelectedDate = day.Date;
            CurrentWeekStart = GetWeekStart(day.Date);
            if (!day.IsCurrentMonth)
                CurrentMonth = new DateTime(day.Date.Year, day.Date.Month, 1);
        }

        // 🔥 CLASE PARA EVENTOS
        public class EventoCalendario
        {
            public Guid TareaId { get; set; }
            public string Nombre { get; set; } = "";
            public DateTime Fecha { get; set; }
            public string Hora { get; set; } = "";
            public string TipoEvento { get; set; } = ""; // "INICIO" o "FIN"
            public TareaModel? TareaRef { get; set; }
        }

        public class CalendarDay
        {
            public DateTime Date { get; set; }
            public int Day { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsToday { get; set; }
            public bool IsSelected { get; set; }
            public bool TieneTareaPorIniciar { get; set; }
            public bool TieneTareaPorFinalizar { get; set; }
        }
    }
}
