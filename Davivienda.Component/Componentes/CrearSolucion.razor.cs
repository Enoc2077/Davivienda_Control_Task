using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class CrearSolucion
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        // FriccionId puede venir del componente padre (DetalleTarea)
        // pero ahora el usuario tambien puede elegir desde el select
        [Parameter] public Guid FriccionId { get; set; }

        // TareaId para filtrar las fricciones de esa tarea
        [Parameter] public Guid TareaId { get; set; }

        [Parameter] public EventCallback OnSuccess { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        // Lista de fricciones disponibles para la tarea
        public List<FriccionModel> FriccionesDisponibles { get; set; } = new();

        // ID de friccion seleccionado en el select (string para manejar el valor vacio)
        private string friccionSeleccionadaId = "";

        private SolucionesModel nuevaSolucion = new SolucionesModel
        {
            SOL_EST = "Pendiente",
            SOL_NIV_EFE = 50,
            SOL_DES = ""
        };

        protected override async Task OnInitializedAsync()
        {
            // Si viene un FriccionId del padre, lo preseleccionamos
            if (FriccionId != Guid.Empty)
                friccionSeleccionadaId = FriccionId.ToString();

            await CargarFricciones();
        }

        private async Task CargarFricciones()
        {
            try
            {
                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesDisponibles = resFri.Data?.Fricciones?
                    .Where(f => f.Tar_ID == TareaId)
                    .Select(f => new FriccionModel
                    {
                        FRI_ID = f.Fri_ID,
                        FRI_TIP = f.Fri_TIP,
                        FRI_EST = f.Fri_EST,
                        FRI_IMP = f.Fri_IMP,
                        TAR_ID = f.Tar_ID
                    }).ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando fricciones: {ex.Message}");
            }
            StateHasChanged();
        }

        private async Task CerrarModalInterno()
        {
            if (OnClose.HasDelegate)
                await OnClose.InvokeAsync();
        }

        private async Task GuardarSolucion()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nuevaSolucion.SOL_NOM)) return;

                // Resolver el ID de friccion seleccionado
                Guid? frId = null;
                if (!string.IsNullOrEmpty(friccionSeleccionadaId) &&
                    Guid.TryParse(friccionSeleccionadaId, out Guid parsedId))
                {
                    frId = parsedId;
                }

                var input = new SolucionesModelInput
                {
                    Sol_ID = Guid.NewGuid(),
                    Sol_NOM = nuevaSolucion.SOL_NOM,
                    Sol_DES = nuevaSolucion.SOL_DES,
                    Sol_EST = nuevaSolucion.SOL_EST ?? "Pendiente",
                    Sol_NIV_EFE = nuevaSolucion.SOL_NIV_EFE ?? 50,
                    Fri_ID = frId,   // null si no se selecciono ninguna
                    Usu_ID = Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD"),
                    Sol_FEC_CRE = DateTimeOffset.Now
                };

                var result = await Client.InsertSolucion.ExecuteAsync(input);

                if (result.Errors.Count == 0)
                    await OnSuccess.InvokeAsync();
                else
                    foreach (var err in result.Errors)
                        Console.WriteLine($"Error GraphQL: {err.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error insertando solución: {ex.Message}");
            }
        }
    }
}