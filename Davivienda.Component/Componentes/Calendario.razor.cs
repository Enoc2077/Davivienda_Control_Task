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
    public partial class Calendario
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        private DateTime CurrentMonth { get; set; } = DateTime.Today;
        private DateTime SelectedDate { get; set; } = DateTime.Today;
        private DateTime CurrentWeekStart { get; set; }
        private List<TareaModel>? ListaTareas { get; set; }

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
                    ListaTareas = response.Data.Tareas.Select(t => new TareaModel
                    {
                        TAR_ID = t.Tar_ID,
                        TAR_NOM = t.Tar_NOM,
                        TAR_DES = t.Tar_DES,
                        TAR_EST = t.Tar_EST,
                        TAR_FEC_INI = t.Tar_FEC_INI,
                        TAR_FEC_FIN = t.Tar_FEC_FIN,
                        PRI_ID = t.Pri_ID
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar tareas: {ex.Message}");
            }
        }

        private string GetEventoPosicion(DateTime inicio, DateTime? fin)
        {
            // Definimos que el inicio visual del calendario es a las 8:00 AM (Punto 0)
            const int HORA_INICIO_VISUAL = 8;

            // Calculamos la hora de inicio en formato decimal (ej: 8:30 = 8.5)
            double horaInicioDecimal = inicio.Hour + (inicio.Minute / 60.0);

            // Calculamos la duración. Si no tiene fin, asignamos 1 hora por defecto
            double duracionHoras = fin.HasValue ? (fin.Value - inicio).TotalHours : 1.0;

            // Si la duración es menor a 30 min, forzamos 0.5 para que el bloque sea visible
            if (duracionHoras <= 0) duracionHoras = 0.5;

            // Calculamos los píxeles (basado en 60px por cada hora)
            // El 'top' es la diferencia entre la hora de la tarea y las 8:00 AM
            double top = (horaInicioDecimal - HORA_INICIO_VISUAL) * 60;
            double height = duracionHoras * 60;

            // Retornamos el estilo CSS para posicionar el bloque al centro de la franja
            return $"top: {top}px; height: {height}px; z-index: 10;";
        }

        private string GetEventoClase(Guid? priId)
        {
            if (priId == null) return "event-baja";

            var id = priId.ToString().ToUpper();
            // Mapeo dinámico de colores por prioridad
            if (id.Contains("ALTA") || id == "1") return "event-alta";
            if (id.Contains("MEDIA") || id == "2") return "event-media";
            return "event-baja";
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private List<CalendarDay> GetMonthDays()
        {
            var days = new List<CalendarDay>();
            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int daysFromMonday = ((int)firstDayOfMonth.DayOfWeek - 1 + 7) % 7;
            var startDate = firstDayOfMonth.AddDays(-daysFromMonday);

            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);
                days.Add(new CalendarDay
                {
                    Date = date,
                    Day = date.Day,
                    IsCurrentMonth = date.Month == CurrentMonth.Month,
                    IsToday = date.Date == DateTime.Today,
                    IsSelected = date.Date == SelectedDate.Date
                });
            }
            return days;
        }

        private List<DateTime> GetWeekDays()
        {
            var days = new List<DateTime>();
            for (int i = 0; i < 7; i++) days.Add(CurrentWeekStart.AddDays(i));
            return days;
        }

        private string GetWeekRange()
        {
            var endDate = CurrentWeekStart.AddDays(6);
            return $"{CurrentWeekStart.ToString("MMMM d")} - {endDate.ToString("d, yyyy")}";
        }

        private int GetWeekNumber() => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(CurrentWeekStart, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

        private void PreviousMonth() => CurrentMonth = CurrentMonth.AddMonths(-1);
        private void NextMonth() => CurrentMonth = CurrentMonth.AddMonths(1);
        private void PreviousWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        private void NextWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(7);

        private void SelectDay(CalendarDay day)
        {
            SelectedDate = day.Date;
            CurrentWeekStart = GetWeekStart(day.Date);
            if (!day.IsCurrentMonth) CurrentMonth = new DateTime(day.Date.Year, day.Date.Month, 1);
        }

        public class CalendarDay
        {
            public DateTime Date { get; set; }
            public int Day { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsToday { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}