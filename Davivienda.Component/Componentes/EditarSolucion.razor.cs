using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;

namespace Davivienda.Component.Componentes
{
    public partial class EditarSolucion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public SolucionesModel SolucionOriginal { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; }

        private SolucionesModel solEdit = new();

        protected override void OnInitialized()
        {
            solEdit = new SolucionesModel
            {
                SOL_ID = SolucionOriginal.SOL_ID,
                SOL_NOM = SolucionOriginal.SOL_NOM,
                SOL_DES = SolucionOriginal.SOL_DES,
                SOL_EST = SolucionOriginal.SOL_EST,
                SOL_NIV_EFE = SolucionOriginal.SOL_NIV_EFE,
                FRI_ID = SolucionOriginal.FRI_ID,
                USU_ID = SolucionOriginal.USU_ID,
                SOL_FEC_CRE = SolucionOriginal.SOL_FEC_CRE // Vital para evitar errores de esquema
            };
        }

        private async Task ActualizarSolucion()
        {
            var input = new SolucionesModelInput
            {
                Sol_ID = solEdit.SOL_ID,
                Sol_NOM = solEdit.SOL_NOM,
                Sol_DES = solEdit.SOL_DES,
                Sol_EST = solEdit.SOL_EST,
                Sol_NIV_EFE = solEdit.SOL_NIV_EFE,
                Fri_ID = solEdit.FRI_ID,
                Usu_ID = solEdit.USU_ID,
                Sol_FEC_CRE = solEdit.SOL_FEC_CRE,
                Sol_FEC_MOD = DateTimeOffset.Now
            };

            var result = await Client.UpdateSolucion.ExecuteAsync(input);
            if (result.Data?.UpdateSolucion ?? false) await OnSuccess.InvokeAsync();
        }
    }
}