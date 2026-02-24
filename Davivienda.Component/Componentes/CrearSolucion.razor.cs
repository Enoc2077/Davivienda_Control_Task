using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class CrearSolucion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public Guid FriccionId { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private SolucionesModel nuevaSolucion = new SolucionesModel
        {
            SOL_EST = "Pendiente",
            SOL_NIV_EFE = 0
        };

        private async Task CerrarModalInterno()
        {
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
        }

        // Ajuste en GuardarSolucion
        private async Task GuardarSolucion()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nuevaSolucion.SOL_NOM)) return;

                var input = new SolucionesModelInput
                {
                    Sol_ID = Guid.NewGuid(),
                    Sol_NOM = nuevaSolucion.SOL_NOM,
                    Sol_DES = nuevaSolucion.SOL_DES,
                    Sol_EST = nuevaSolucion.SOL_EST ?? "Pendiente",
                    Sol_NIV_EFE = nuevaSolucion.SOL_NIV_EFE ?? 0,
                    // Si FriccionId es Guid.Empty, pasamos null para que la DB lo entienda
                    Fri_ID = FriccionId == Guid.Empty ? null : FriccionId,
                    Usu_ID = Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD"),
                    Sol_FEC_CRE = DateTimeOffset.Now
                };

                var result = await Client.InsertSolucion.ExecuteAsync(input);
                // ... éxito ...
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }
    }
}