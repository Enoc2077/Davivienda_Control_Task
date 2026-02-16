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
    public partial class EditarTarea
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public TareaModel Tarea { get; set; } = new();
        [Parameter] public EventCallback OnClose { get; set; }

        private List<UsuarioModel> ListaUsuarios = new();

        protected override async Task OnInitializedAsync()
        {
            // Validamos si la fecha viene vacía (01/01/0001) para evitar el error visual de ceros
            if (Tarea.TAR_FEC_INI == DateTime.MinValue || Tarea.TAR_FEC_INI == null)
            {
                Tarea.TAR_FEC_INI = DateTime.Now;
            }

            if (Tarea.TAR_FEC_FIN == DateTime.MinValue || Tarea.TAR_FEC_FIN == null)
            {
                Tarea.TAR_FEC_FIN = DateTime.Now.AddDays(1);
            }

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
                Console.WriteLine($"Error usuarios: {ex.Message}");
            }
        }

        public async Task CerrarModal() => await OnClose.InvokeAsync();

        public async Task ActualizarTarea()
        {
            try
            {
                var input = new TareaModelInput
                {
                    Tar_ID = Tarea.TAR_ID,
                    Tar_NOM = Tarea.TAR_NOM,
                    Tar_DES = Tarea.TAR_DES,
                    Tar_EST = Tarea.TAR_EST,
                    // Aseguramos que se envíe la fecha capturada en el input yyyy-MM-dd
                    Tar_FEC_INI = Tarea.TAR_FEC_INI,
                    Tar_FEC_FIN = Tarea.TAR_FEC_FIN,
                    Proc_ID = Tarea.PROC_ID,
                    Pri_ID = Tarea.PRI_ID,
                    Usu_ID = Tarea.USU_ID,
                    Tar_FEC_MOD = DateTimeOffset.Now
                };
                await Client.UpdateTarea.ExecuteAsync(input);
                await OnClose.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}