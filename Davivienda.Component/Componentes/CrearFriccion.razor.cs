using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class CrearFriccion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public Guid TareaId { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private FriccionModel nuevaFriccion = new FriccionModel
        {
            FRI_EST = "Abierta",
            FRI_IMP = "Medio"
        };

        private async Task CerrarModalInterno()
        {
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
        }

        private async Task GuardarFriccion()
        {
            try
            {
                // Validación básica manual ya que no usamos EditForm
                if (string.IsNullOrWhiteSpace(nuevaFriccion.FRI_TIP)) return;

                var input = new FriccionModelInput
                {
                    Fri_ID = Guid.NewGuid(),
                    Fri_TIP = nuevaFriccion.FRI_TIP,
                    Fri_DES = nuevaFriccion.FRI_DES,
                    Fri_EST = nuevaFriccion.FRI_EST,
                    Fri_IMP = nuevaFriccion.FRI_IMP,
                    Tar_ID = TareaId,
                    Usu_ID = Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD"),
                    Fri_FEC_CRE = DateTimeOffset.Now
                };

                var result = await Client.InsertFriccion.ExecuteAsync(input);

                if (result.Data?.InsertFriccion ?? false)
                {
                    await OnSuccess.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error insertando fricción: {ex.Message}");
            }
        }
    }
}