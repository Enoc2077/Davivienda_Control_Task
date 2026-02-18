using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Davivienda.Component.Componentes
{
    public partial class Graficas
    {
        [Parameter] public string TipoGrafica { get; set; } = "";
        [Parameter] public int TotalTareas { get; set; }

        // Evento para comunicar el filtro a la página principal
        [Parameter] public EventCallback<string> OnFiltrar { get; set; }

        [Parameter] public int AltaCount { get; set; }
        [Parameter] public int MediaCount { get; set; }
        [Parameter] public int BajaCount { get; set; }

        [Parameter] public int TotalProyectos { get; set; }
        [Parameter] public List<ProyectosModel>? Proyectos { get; set; }

        [Parameter] public int Pendientes { get; set; }
        [Parameter] public int EnProgreso { get; set; }
        [Parameter] public int Completadas { get; set; }

        private string GetPieStyle()
        {
            if (TotalTareas == 0)
                return "background: #E5E7EB;";

            double pAlta = (AltaCount * 100.0) / TotalTareas;
            double pMedia = (MediaCount * 100.0) / TotalTareas;

            // CRÍTICO: Usar colores HEX directos, NO variables CSS
            return $"background: conic-gradient(" +
                   $"#EF4444 0% {pAlta}%, " +                    // Rojo
                   $"white {pAlta}% {pAlta + 0.5}%, " +          // Separador
                   $"#F59E0B {pAlta + 0.5}% {pAlta + pMedia}%, " + // Amarillo
                   $"white {pAlta + pMedia}% {pAlta + pMedia + 0.5}%, " + // Separador
                   $"#10B981 {pAlta + pMedia + 0.5}% 100%);";   // Verde
        }

        private double GetBarHeight(int count)
        {
            const double META_VISUAL = 20.0;
            double altura = (count / META_VISUAL) * 100;
            return altura > 100 ? 100 : (altura < 15 ? 15 : altura);
        }
    }
}