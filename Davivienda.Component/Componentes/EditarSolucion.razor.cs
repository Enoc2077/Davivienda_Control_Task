using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Davivienda.GraphQL.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class EditarSolucion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public SolucionesModel Solucion { get; set; } = default!;

        // Lista de fricciones ya filtrada por la tarea — viene de DetalleTarea
        [Parameter] public List<FriccionModel> FriccionesDisponibles { get; set; } = new();

        [Parameter] public EventCallback OnSuccess { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private SolucionesModel solEdit = new();
        private string friccionSeleccionadaId = "";

        protected override void OnInitialized()
        {
            if (Solucion != null)
            {
                solEdit = new SolucionesModel
                {
                    SOL_ID = Solucion.SOL_ID,
                    SOL_NOM = Solucion.SOL_NOM,
                    SOL_DES = Solucion.SOL_DES,
                    SOL_EST = Solucion.SOL_EST,
                    SOL_NIV_EFE = Solucion.SOL_NIV_EFE,
                    FRI_ID = Solucion.FRI_ID,
                    USU_ID = Solucion.USU_ID,
                    SOL_FEC_CRE = Solucion.SOL_FEC_CRE
                };

                // Preseleccionar la friccion actual
                if (Solucion.FRI_ID.HasValue && Solucion.FRI_ID != Guid.Empty)
                    friccionSeleccionadaId = Solucion.FRI_ID.Value.ToString();
            }
        }

        private async Task ActualizarSolucion()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(solEdit.SOL_NOM)) return;

                Guid? frId = null;
                if (!string.IsNullOrEmpty(friccionSeleccionadaId) &&
                    Guid.TryParse(friccionSeleccionadaId, out Guid parsed))
                    frId = parsed;

                var input = new SolucionesModelInput
                {
                    Sol_ID = solEdit.SOL_ID,
                    Sol_NOM = solEdit.SOL_NOM,
                    Sol_DES = solEdit.SOL_DES,
                    Sol_EST = solEdit.SOL_EST ?? "Pendiente",
                    Sol_NIV_EFE = solEdit.SOL_NIV_EFE ?? 0,
                    Fri_ID = frId,
                    Usu_ID = solEdit.USU_ID,
                    Sol_FEC_CRE = solEdit.SOL_FEC_CRE,
                    Sol_FEC_MOD = DateTimeOffset.Now
                };

                var result = await Client.UpdateSolucion.ExecuteAsync(input);
                if (result.Data?.UpdateSolucion ?? false)
                    await OnSuccess.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar solución: {ex.Message}");
            }
        }

        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}