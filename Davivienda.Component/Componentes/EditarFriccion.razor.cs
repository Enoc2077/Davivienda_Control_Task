using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class EditarFriccion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public FriccionModel FriccionOriginal { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; }

        private FriccionModel friccionEdit = new();

        protected override void OnInitialized()
        {
            // Clonamos los datos originales incluyendo la fecha de creación obligatoria
            friccionEdit = new FriccionModel
            {
                FRI_ID = FriccionOriginal.FRI_ID,
                FRI_TIP = FriccionOriginal.FRI_TIP,
                FRI_DES = FriccionOriginal.FRI_DES,
                FRI_EST = FriccionOriginal.FRI_EST,
                FRI_IMP = FriccionOriginal.FRI_IMP,
                TAR_ID = FriccionOriginal.TAR_ID,
                USU_ID = FriccionOriginal.USU_ID,
                FRI_FEC_CRE = FriccionOriginal.FRI_FEC_CRE // Requerido por el esquema
            };
        }

        private async Task ActualizarFriccion()
        {
            try
            {
                var input = new FriccionModelInput
                {
                    Fri_ID = friccionEdit.FRI_ID,
                    Fri_TIP = friccionEdit.FRI_TIP,
                    Fri_DES = friccionEdit.FRI_DES,
                    Fri_EST = friccionEdit.FRI_EST,
                    Fri_IMP = friccionEdit.FRI_IMP,
                    Tar_ID = friccionEdit.TAR_ID,
                    Usu_ID = friccionEdit.USU_ID,
                    Fri_FEC_CRE = friccionEdit.FRI_FEC_CRE, // Enviamos la fecha original
                    Fri_FEC_MOD = DateTimeOffset.Now // Registramos la fecha de edición
                };

                var result = await Client.UpdateFriccion.ExecuteAsync(input);

                if (result.Data?.UpdateFriccion ?? false)
                {
                    await OnSuccess.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar: {ex.Message}");
            }
        }
    }
}