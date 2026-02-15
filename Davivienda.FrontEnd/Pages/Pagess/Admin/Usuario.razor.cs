using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Davivienda.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop; // Necesario para la confirmación

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public partial class Usuario
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; }
        [Inject] private IJSRuntime JS { get; set; } // Injectamos el runtime de JavaScript

        private List<UsuarioModel> Usuarios { get; set; } = new();
        private List<UsuarioModel> UsuariosFiltrados { get; set; } = new();
        private List<RolesModel> Roles { get; set; } = new();

        private UsuarioModel UsuarioForm { get; set; } = new();
        private string Busqueda { get; set; } = "";
        private bool Editando { get; set; } = false;
        private bool MostrarModal { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                var resUser = await Client.GetUsuarios.ExecuteAsync();
                Usuarios = resUser.Data?.Usuarios.Select(u => new UsuarioModel
                {
                    USU_ID = u.Usu_ID,
                    USU_NOM = u.Usu_NOM,
                    USU_NUM = u.Usu_NUM,
                    USU_COR = u.Usu_COR,
                    USU_TEL = u.Usu_TEL,
                    USU_EST = u.Usu_EST,
                    ROL_ID = u.Rol_ID,
                    USU_FEC_CRE = u.Usu_FEC_CRE,
                    USU_FEC_MOD = u.Usu_FEC_MOD
                }).ToList() ?? new();

                var resRoles = await Client.GetRoles.ExecuteAsync();
                Roles = resRoles.Data?.Roles.Select(r => new RolesModel
                {
                    ROL_ID = r.Rol_ID,
                    ROL_NOM = r.Rol_NOM
                }).ToList() ?? new();

                FiltrarUsuarios();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        private void FiltrarUsuarios()
        {
            UsuariosFiltrados = string.IsNullOrWhiteSpace(Busqueda)
                ? Usuarios
                : Usuarios.Where(u => u.USU_NOM.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // --- MÉTODOS DE ELIMINACIÓN CON CONFIRMACIÓN ---
        private async Task EliminarUsuario(Guid id)
        {
            // 1. Buscamos el nombre para el mensaje
            var usuario = Usuarios.FirstOrDefault(u => u.USU_ID == id);
            string nombre = usuario?.USU_NOM ?? "este usuario";

            // 2. Mostramos confirmación al usuario
            bool confirmado = await JS.InvokeAsync<bool>("confirm", $"¿Está seguro que desea eliminar a {nombre}? Esta acción no se puede deshacer.");

            if (confirmado)
            {
                try
                {
                    // 3. Llamada Real a la Base de Datos vía GraphQL
                    // Asegúrate de que tu mutación en el .graphql se llame DeleteUsuario
                    var resultado = await Client.DeleteUsuario.ExecuteAsync(id);

                    if (resultado.Errors.Count == 0)
                    {
                        // 4. Si fue exitoso en DB, refrescamos la lista local
                        await CargarDatos();
                        StateHasChanged();
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("alert", "Error al eliminar en la base de datos.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en el proceso de eliminación: {ex.Message}");
                }
            }
            // Si el usuario le da a "Cancelar", el método termina aquí y no pasa nada.
        }

        private void AbrirModalNuevo()
        {
            UsuarioForm = new UsuarioModel
            {
                USU_ID = Guid.NewGuid(),
                USU_EST = true,
                USU_FEC_CRE = DateTimeOffset.Now
            };
            Editando = false;
            MostrarModal = true;
        }

        private void AbrirModalEditar(UsuarioModel user)
        {
            UsuarioForm = new UsuarioModel
            {
                USU_ID = user.USU_ID,
                USU_NOM = user.USU_NOM,
                USU_NUM = user.USU_NUM,
                USU_COR = user.USU_COR,
                USU_TEL = user.USU_TEL,
                USU_EST = user.USU_EST,
                ROL_ID = user.ROL_ID,
                USU_FEC_CRE = user.USU_FEC_CRE
            };
            Editando = true;
            MostrarModal = true;
        }

        private async Task GuardarCambios()
        {
            try
            {
                var input = new UsuarioModelInput
                {
                    Usu_ID = UsuarioForm.USU_ID,
                    Usu_NOM = UsuarioForm.USU_NOM,
                    Usu_NUM = UsuarioForm.USU_NUM,
                    Usu_COR = UsuarioForm.USU_COR,
                    Usu_TEL = UsuarioForm.USU_TEL,
                    Usu_EST = UsuarioForm.USU_EST,
                    Rol_ID = UsuarioForm.ROL_ID,
                    Usu_FEC_MOD = DateTimeOffset.Now
                };

                if (Editando)
                {
                    input.Usu_FEC_CRE = UsuarioForm.USU_FEC_CRE;
                    await Client.UpdateUsuario.ExecuteAsync(input);
                }
                else
                {
                    input.Usu_FEC_CRE = DateTimeOffset.Now;
                    await Client.InsertUsuario.ExecuteAsync(input);
                }

                MostrarModal = false;
                await CargarDatos();
                StateHasChanged();
            }
            catch (Exception ex) { Console.WriteLine($"Error al guardar: {ex.Message}"); }
        }
    }
}