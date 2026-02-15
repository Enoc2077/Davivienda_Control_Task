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

        private List<AreasModel> ListaAreas = new();

        protected override async Task OnInitializedAsync()
        {
            await CargarAreas();
        }

        private async Task CargarAreas()
        {
            try
            {
                var res = await Client.GetAreas.ExecuteAsync();
                if (res.Data?.Areas != null)
                {
                    ListaAreas = res.Data.Areas.Select(a => new AreasModel
                    {
                        ARE_ID = a.Are_ID,
                        ARE_NOM = a.Are_NOM
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar áreas: {ex.Message}");
            }
        }

        public async Task CerrarModal() => await OnClose.InvokeAsync();

        public async Task ActualizarProyecto()
        {
            try
            {
                // VALIDACIÓN CRÍTICA: Si el ARE_ID es nulo o Guid.Empty, el servidor lanzará el error de FK
                if (Proyecto.ARE_ID == null || Proyecto.ARE_ID == Guid.Empty)
                {
                    // Aquí podrías mostrar una alerta al usuario
                    Console.WriteLine("Error: Debe seleccionar un área válida.");
                    return;
                }

                var input = new ProyectosModelInput
                {
                    Pro_ID = Proyecto.PRO_ID,
                    Pro_NOM = Proyecto.PRO_NOM,
                    Pro_DES = Proyecto.PRO_DES,
                    Pro_FEC_INI = Proyecto.PRO_FEC_INI,
                    Pro_FEC_FIN = Proyecto.PRO_FEC_FIN,
                    Pro_EST = Proyecto.PRO_EST,
                    Are_ID = Proyecto.ARE_ID, // Enviamos el ID seleccionado del select
                    Pro_FEC_CRE = Proyecto.PRO_FEC_CRE,
                    Pro_FEC_MOD = DateTimeOffset.Now
                };

                var res = await Client.UpdateProyecto.ExecuteAsync(input);

                if (res.Errors.Any())
                {
                    // Esto capturará el error de base de datos y lo mostrará en consola para depurar
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