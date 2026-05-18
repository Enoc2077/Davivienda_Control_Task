using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Davivienda.GraphQL.SDK;

namespace Davivienda.Component.Componentes
{
    public partial class EditarProyecto : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public ProyectosModel Proyecto { get; set; } = new();
        [Parameter] public EventCallback OnClose { get; set; }

        public async Task CerrarModal() => await OnClose.InvokeAsync();

        public async Task ActualizarProyecto()
        {
            try
            {
                var input = new ProyectosModelInput
                {
                    Pro_ID = Proyecto.PRO_ID,
                    Pro_NOM = Proyecto.PRO_NOM,
                    Pro_DES = Proyecto.PRO_DES,
                    Pro_FEC_INI = Proyecto.PRO_FEC_INI,
                    Pro_FEC_FIN = Proyecto.PRO_FEC_FIN,
                    Pro_EST = Proyecto.PRO_EST,
                    Are_ID = Proyecto.ARE_ID,
                    Pro_FEC_CRE = Proyecto.PRO_FEC_CRE,
                    Pro_FEC_MOD = DateTimeOffset.Now
                };

                var res = await Client.UpdateProyecto.ExecuteAsync(input);

                if (res.Errors != null && res.Errors.Any())
                {
                    Console.WriteLine($"Error de GraphQL: {res.Errors.First().Message}");
                }
                else if (res.Data?.UpdateProyecto == true)
                {
                    await OnClose.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la petición: {ex.Message}");
            }
        }
    }
}