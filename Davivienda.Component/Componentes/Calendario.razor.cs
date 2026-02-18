using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private TareaModel? TareaEnFoco;
        private int CountAlta, CountMedia, CountBaja;
        private TareaModel? TareaPrioritaria;

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

                        // Si la tarea no tiene hora (llega a las 00:00), asignamos aleatorio laboral
                        if (fIni.Hour == 0 && fIni.Minute == 0)
                        {
                            int horaRnd = rnd.Next(7, 17); // Entre las 7 AM y las 5 PM
                            int minRnd = rnd.Next(0, 4) * 15;
                            fIni = new DateTime(fIni.Year, fIni.Month, fIni.Day, horaRnd, minRnd, 0);
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

                    ActualizarEstadisticas();
                    TareaEnFoco = TareaPrioritaria;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private string GetEventoPosicion(DateTime inicio, DateTime? fin, int colIndex)
        {
            const int HORA_INICIO_VISUAL = 0;
            double inicioDecimal = inicio.Hour + (inicio.Minute / 60.0);
            double duracion = fin.HasValue ? (fin.Value - inicio).TotalHours : 1.0;

            // Ajuste: Mínimo 45 minutos (45px) para que se vean las dos horas (inicio y fin)
            if (duracion < 0.75) duracion = 0.75;

            double top = (inicioDecimal - HORA_INICIO_VISUAL) * 60;
            double height = duracion * 60;

            int desplazamientoX = colIndex * 20;
            return $"top: {top}px; height: {height}px; left: {desplazamientoX}px; width: calc(100% - {desplazamientoX + 5}px); z-index: {10 + colIndex};";
        }

        private void ActualizarEstadisticas()
        {
            if (ListaTareas == null) return;
            var activas = ListaTareas.Where(t => t.TAR_EST != "Completado").ToList();
            CountAlta = activas.Count(t => GetPrioridadCategoria(t.PRI_ID) == "Alta");
            CountMedia = activas.Count(t => GetPrioridadCategoria(t.PRI_ID) == "Media");
            CountBaja = activas.Count(t => GetPrioridadCategoria(t.PRI_ID) == "Baja");
            TareaPrioritaria = activas.OrderBy(t => t.TAR_FEC_FIN ?? DateTime.MaxValue).ThenBy(t => GetPrioridadPeso(t.PRI_ID)).FirstOrDefault();
        }

        private void EnfocarTarea(TareaModel tarea) { TareaEnFoco = tarea; StateHasChanged(); }
        private string GetEventoClase(Guid? priId) => GetPrioridadCategoria(priId) switch { "Alta" => "event-alta", "Media" => "event-media", _ => "event-baja" };
        private string GetPrioridadCategoria(Guid? priId)
        {
            if (priId == null) return "Baja";
            var idStr = priId.ToString().ToLower();
            if (idStr.Contains("1") || idStr.Contains("alta")) return "Alta";
            if (idStr.Contains("2") || idStr.Contains("media")) return "Media";
            return "Baja";
        }
        private int GetPrioridadPeso(Guid? priId) => GetPrioridadCategoria(priId) == "Alta" ? 0 : 1;
        private DateTime GetWeekStart(DateTime date) { int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7; return date.AddDays(-1 * diff).Date; }
        private List<CalendarDay> GetMonthDays()
        {
            var days = new List<CalendarDay>();
            var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int offset = ((int)firstDay.DayOfWeek - 1 + 7) % 7;
            var start = firstDay.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                var d = start.AddDays(i);

                // Verificamos si en este día hay tareas que inician o terminan
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
        private int GetWeekNumber() => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(CurrentWeekStart, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        private void PreviousMonth() => CurrentMonth = CurrentMonth.AddMonths(-1);
        private void NextMonth() => CurrentMonth = CurrentMonth.AddMonths(1);
        private void PreviousWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        private void NextWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(7);
        private void SelectDay(CalendarDay day) { SelectedDate = day.Date; CurrentWeekStart = GetWeekStart(day.Date); if (!day.IsCurrentMonth) CurrentMonth = new DateTime(day.Date.Year, day.Date.Month, 1); }
        public class CalendarDay
        {
            public DateTime Date { get; set; }
            public int Day { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsToday { get; set; }
            public bool IsSelected { get; set; }
            // NUEVOS ESTADOS VISUALES
            public bool TieneTareaPorIniciar { get; set; }
            public bool TieneTareaPorFinalizar { get; set; }
        }
    }
}