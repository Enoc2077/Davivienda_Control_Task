using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Davivienda.GraphQL.SDK;

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
                fricEdit = new FriccionModel
                {
                    FRI_ID = Friccion.FRI_ID,
                    FRI_TIP = Friccion.FRI_TIP,
                    FRI_DES = Friccion.FRI_DES,
                    FRI_EST = Friccion.FRI_EST,
                    FRI_IMP = Friccion.FRI_IMP,
                    TAR_ID = Friccion.TAR_ID,
                    FRI_FEC_CRE = Friccion.FRI_FEC_CRE
                };
            }
        }

        private async Task ActualizarFriccion()
        {
            var input = new FriccionModelInput
            {
                Fri_ID = fricEdit.FRI_ID,
                Fri_TIP = fricEdit.FRI_TIP,
                Fri_DES = fricEdit.FRI_DES,
                Fri_EST = fricEdit.FRI_EST,
                Fri_IMP = fricEdit.FRI_IMP,
                Tar_ID = fricEdit.TAR_ID,
                Fri_FEC_MOD = DateTimeOffset.Now
            };
            await Client.UpdateFriccion.ExecuteAsync(input);
            await OnSuccess.InvokeAsync();
        }

        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}