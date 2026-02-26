using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Davivienda.GraphQL.SDK;
using System;
using System.Threading.Tasks;

namespace Davivienda.Componentes
{
    public partial class EditarFriccion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public FriccionModel Friccion { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private FriccionModel fricEdit = new();

        protected override void OnInitialized()
        {
            if (Friccion != null)
            {
                // Realizamos una copia del objeto para no modificar el original por referencia antes de guardar
                fricEdit = new FriccionModel
                {
                    FRI_ID = Friccion.FRI_ID,
                    FRI_TIP = Friccion.FRI_TIP,
                    FRI_DES = Friccion.FRI_DES, // El Yoopta recibirá el JSON guardado aquí
                    FRI_EST = Friccion.FRI_EST,
                    FRI_IMP = Friccion.FRI_IMP,
                    TAR_ID = Friccion.TAR_ID,
                    USU_ID = Friccion.USU_ID,
                    FRI_FEC_CRE = Friccion.FRI_FEC_CRE
                };
            }
        }

        private async Task ActualizarFriccion()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fricEdit.FRI_TIP)) return;

                var input = new FriccionModelInput
                {
                    Fri_ID = fricEdit.FRI_ID,
                    Fri_TIP = fricEdit.FRI_TIP,
                    Fri_DES = fricEdit.FRI_DES, // Se envía el JSON actualizado del editor
                    Fri_EST = fricEdit.FRI_EST,
                    Fri_IMP = fricEdit.FRI_IMP,
                    Tar_ID = fricEdit.TAR_ID,
                    Usu_ID = fricEdit.USU_ID,
                    Fri_FEC_CRE = fricEdit.FRI_FEC_CRE,
                    Fri_FEC_MOD = DateTimeOffset.Now
                };

                var result = await Client.UpdateFriccion.ExecuteAsync(input);

                if (result.Errors.Count == 0)
                {
                    await OnSuccess.InvokeAsync();
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error GraphQL al actualizar: {error.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar fricción: {ex.Message}");
            }
        }

        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}