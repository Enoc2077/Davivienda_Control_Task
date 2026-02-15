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

        private FriccionModel nuevaFriccion = new FriccionModel
        {
            FRI_EST = "Abierta",
            FRI_IMP = "Medio"
        };

        private async Task GuardarFriccion()
        {
            try
            {
                // Mapeo al Input del SDK. El nombre se guarda en Fri_TIP.
                var input = new FriccionModelInput
                {
                    Fri_ID = Guid.NewGuid(),
                    Fri_TIP = nuevaFriccion.FRI_TIP,
                    Fri_DES = nuevaFriccion.FRI_DES,
                    Fri_EST = nuevaFriccion.FRI_EST,
                    Fri_IMP = nuevaFriccion.FRI_IMP,
                    Tar_ID = TareaId,
                    Usu_ID = Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD"), // Usuario actual
                    Fri_FEC_CRE = DateTimeOffset.Now
                };

                // El servicio realizará la inserción en FRICCION y BITACORA_FRICCIONES
                var result = await Client.InsertFriccion.ExecuteAsync(input);

                if (result.Data?.InsertFriccion ?? false)
                {
                    await OnSuccess.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar la fricción: {ex.Message}");
            }
        }
    }
}