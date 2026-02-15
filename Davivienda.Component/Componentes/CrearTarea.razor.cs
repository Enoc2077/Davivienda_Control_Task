using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davivienda.GraphQL.SDK;

namespace Davivienda.Component.Componentes
{
    public partial class CrearTarea
    {

        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public Guid ProcId { get; set; } // Recibe el ID del proceso desde el padre
        [Parameter] public EventCallback OnClose { get; set; }

        public TareaModel NuevaTarea { get; set; } = new();
        private List<UsuarioModel> ListaUsuarios = new();

        protected override async Task OnInitializedAsync()
        {
            // Inicializamos valores por defecto
            NuevaTarea = new TareaModel
            {
                PROC_ID = ProcId,
                TAR_EST = "Pendiente",
                TAR_FEC_INI = DateTimeOffset.Now
            };
            await CargarUsuarios();
        }

        private async Task CargarUsuarios()
        {
            try
            {
                var res = await Client.GetUsuarios.ExecuteAsync();
                ListaUsuarios = res.Data?.Usuarios.Select(u => new UsuarioModel
                {
                    USU_ID = u.Usu_ID,
                    USU_NOM = u.Usu_NOM
                }).ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar usuarios: {ex.Message}");
            }
        }

        public async Task GuardarNuevaTarea()
        {
            try
            {
                var input = new TareaModelInput
                {
                    Tar_ID = Guid.NewGuid(), // Generamos nuevo ID
                    Tar_NOM = NuevaTarea.TAR_NOM,
                    Tar_DES = NuevaTarea.TAR_DES,
                    Tar_EST = NuevaTarea.TAR_EST,
                    Tar_FEC_INI = NuevaTarea.TAR_FEC_INI,
                    Tar_FEC_FIN = NuevaTarea.TAR_FEC_FIN,
                    Proc_ID = ProcId, // Vinculado al proceso actual
                    Usu_ID = NuevaTarea.USU_ID,
                    Tar_FEC_CRE = DateTimeOffset.Now,
                    Tar_FEC_MOD = DateTimeOffset.Now
                };

                // Llamada a la mutación de inserción (Asegúrate que InsertTarea exista en tu SDK)
                await Client.InsertTarea.ExecuteAsync(input);
                await OnClose.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear tarea: {ex.Message}");
            }
        }

        public Task CerrarModal() => OnClose.InvokeAsync();
    }
}