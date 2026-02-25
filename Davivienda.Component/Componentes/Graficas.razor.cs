using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Davivienda.Component.Componentes
{
    public partial class Graficas
    {
        [Parameter] public string TipoGrafica { get; set; } = "Estados";
        [Parameter] public int TotalTareas { get; set; }
        [Parameter] public int Pendientes { get; set; }
        [Parameter] public int EnProgreso { get; set; }
        [Parameter] public int Completadas { get; set; }
        [Parameter] public EventCallback<string> OnFiltrar { get; set; }

        private double CalcularAltura(int cantidad)
        {
            if (TotalTareas == 0) return 0;
            double porcentaje = ((double)cantidad / TotalTareas) * 100;
            // Mínimo 15% para que siempre se vea algo
            return Math.Max(porcentaje, cantidad > 0 ? 15 : 0);
        }

        private int CalcularPorcentaje(int cantidad)
        {
            if (TotalTareas == 0) return 0;
            return (int)Math.Round(((double)cantidad / TotalTareas) * 100);
        }
    }
}