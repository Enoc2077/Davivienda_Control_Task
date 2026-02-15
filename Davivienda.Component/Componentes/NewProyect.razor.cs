using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Component
{
    public partial class NewProyect
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public ProyectosModel Proyecto { get; set; } = new();
        [Parameter] public EventCallback OnClose { get; set; }

        private List<AreasModel> ListaAreas = new();

        protected override async Task OnInitializedAsync()
        {
            await CargarAreas();

            // Inicialización de fechas a DateTimeOffset para el API
            if (Proyecto.PRO_FEC_INI == default)
                Proyecto.PRO_FEC_INI = DateTimeOffset.Now;

            if (Proyecto.PRO_FEC_CRE == default)
                Proyecto.PRO_FEC_CRE = DateTimeOffset.Now;
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

        public async Task GuardarProyecto()
        {
            try
            {
                var input = new ProyectosModelInput
                {
                    // Solo los campos que el esquema actualizado permite
                    Pro_ID = Proyecto.PRO_ID == Guid.Empty ? Guid.Empty : Proyecto.PRO_ID,
                    Pro_NOM = Proyecto.PRO_NOM,
                    Pro_DES = Proyecto.PRO_DES,
                    Pro_FEC_INI = Proyecto.PRO_FEC_INI,
                    Pro_FEC_FIN = Proyecto.PRO_FEC_FIN,
                    Pro_EST = Proyecto.PRO_EST,
                    Are_ID = Proyecto.ARE_ID,
                    Pro_FEC_CRE = Proyecto.PRO_FEC_CRE,
                    Pro_FEC_MOD = DateTimeOffset.Now
                };

                if (Proyecto.PRO_ID == Guid.Empty)
                    await Client.InsertProyecto.ExecuteAsync(input);
                else
                    await Client.UpdateProyecto.ExecuteAsync(input);

                await OnClose.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error tras regenerar: {ex.Message}");
            }
        }
    }
}