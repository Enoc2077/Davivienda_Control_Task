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

        private SolucionesModel nuevaSolucion = new SolucionesModel();

        private async Task GuardarSolucion()
        {
            try
            {
                // Mapeo manual al objeto de entrada del SDK
                // Se genera un nuevo ID y se asigna la fecha actual para cumplir con el esquema
                var input = new SolucionesModelInput
                {
                    Sol_ID = Guid.NewGuid(),
                    Sol_NOM = nuevaSolucion.SOL_NOM,
                    Sol_DES = nuevaSolucion.SOL_DES,
                    Sol_EST = nuevaSolucion.SOL_EST ?? "Pendiente",
                    Sol_NIV_EFE = nuevaSolucion.SOL_NIV_EFE ?? 0,
                    Fri_ID = FriccionId,
                    Usu_ID = Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD"),
                    Sol_FEC_CRE = DateTimeOffset.Now
                };

                var result = await Client.InsertSolucion.ExecuteAsync(input);

                if (result.Data?.InsertSolucion ?? false)
                {
                    // Notifica al componente padre para cerrar el modal y refrescar la lista
                    await OnSuccess.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico al guardar la solución: {ex.Message}");
            }
        }
    }
}