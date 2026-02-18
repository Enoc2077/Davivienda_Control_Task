using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Davivienda.GraphQL.SDK;
using System;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class EditarSolucion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public SolucionesModel Solucion { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private SolucionesModel solEdit = new();

        protected override void OnInitialized()
        {
            if (Solucion != null)
            {
                // Clonamos con TODOS los campos para que el @bind en el HTML tenga qué mostrar
                solEdit = new SolucionesModel
                {
                    SOL_ID = Solucion.SOL_ID,
                    SOL_NOM = Solucion.SOL_NOM,
                    SOL_DES = Solucion.SOL_DES,
                    SOL_EST = Solucion.SOL_EST,
                    SOL_NIV_EFE = Solucion.SOL_NIV_EFE,
                    FRI_ID = Solucion.FRI_ID,
                    USU_ID = Solucion.USU_ID,
                    SOL_FEC_CRE = Solucion.SOL_FEC_CRE // Mantiene la fecha original
                };
            }
        }

        private async Task ActualizarSolucion()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(solEdit.SOL_NOM)) return;

                // Creamos el input incluyendo los campos que GraphQL marca como obligatorios
                var input = new SolucionesModelInput
                {
                    Sol_ID = solEdit.SOL_ID,
                    Sol_NOM = solEdit.SOL_NOM,
                    Sol_DES = solEdit.SOL_DES,
                    Sol_EST = solEdit.SOL_EST ?? "Pendiente",
                    Sol_NIV_EFE = solEdit.SOL_NIV_EFE ?? 0,
                    Fri_ID = solEdit.FRI_ID,
                    Usu_ID = solEdit.USU_ID,
                    // Enviamos la fecha original para que no de error de "missing field"
                    Sol_FEC_CRE = solEdit.SOL_FEC_CRE,
                    Sol_FEC_MOD = DateTimeOffset.Now
                };

                var result = await Client.UpdateSolucion.ExecuteAsync(input);

                if (result.Data?.UpdateSolucion ?? false)
                {
                    await OnSuccess.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar solución: {ex.Message}");
            }
        }

        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}