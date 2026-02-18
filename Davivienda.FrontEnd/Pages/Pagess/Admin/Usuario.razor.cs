using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Davivienda.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public partial class Usuario
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private List<UsuarioModel> Usuarios { get; set; } = new();
        private List<UsuarioModel> UsuariosFiltrados { get; set; } = new();

        private UsuarioModel UsuarioForm { get; set; } = new();
        private string Busqueda { get; set; } = "";
        private bool Editando { get; set; } = false;
        private bool MostrarModal { get; set; } = false;

        private string PasswordCheck { get; set; } = "";
        private string PasswordReal { get; set; } = "";
        private bool ErrorPass { get; set; } = false;

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
                    USU_CON = u.Usu_CON,
                    USU_TEL = u.Usu_TEL,
                    USU_EST = u.Usu_EST,
                    ROL_ID = u.Rol_ID,
                    ARE_ID = u.Are_ID,
                    USU_FEC_CRE = u.Usu_FEC_CRE
                }).ToList() ?? new();

                FiltrarUsuarios();
            }
            catch (Exception ex) { Console.WriteLine($"Error al cargar: {ex.Message}"); }
        }

        private void FiltrarUsuarios()
        {
            UsuariosFiltrados = string.IsNullOrWhiteSpace(Busqueda)
                ? Usuarios
                : Usuarios.Where(u => u.USU_NOM.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void AbrirModalNuevo()
        {
            // Auto-generación de número de empleado (5 dígitos)
            // Buscamos el mayor número, le sumamos 1 y rellenamos con ceros a la izquierda
            int maxNum = 0;
            if (Usuarios.Any())
            {
                maxNum = Usuarios.Max(u => int.TryParse(u.USU_NUM, out int n) ? n : 0);
            }
            else
            {
                maxNum = 100; // Valor inicial si no hay nadie
            }

            string nuevoNum = (maxNum + 1).ToString().PadLeft(5, '0');

            UsuarioForm = new UsuarioModel
            {
                USU_ID = Guid.NewGuid(),
                USU_NUM = nuevoNum,
                USU_EST = true // Por defecto activo
            };

            PasswordCheck = "";
            ErrorPass = false;
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
                ARE_ID = user.ARE_ID,
                USU_CON = user.USU_CON,
                USU_FEC_CRE = user.USU_FEC_CRE
            };
            PasswordReal = user.USU_CON;
            PasswordCheck = "";
            ErrorPass = false;
            Editando = true;
            MostrarModal = true;
        }

        private async Task ValidarYGuardar()
        {
            ErrorPass = false;

            // 1. Seguridad Contraseña
            if (Editando && PasswordCheck != PasswordReal) { ErrorPass = true; return; }
            if (!Editando && string.IsNullOrWhiteSpace(PasswordCheck)) { ErrorPass = true; return; }

            // 2. Obligatorios
            if (string.IsNullOrWhiteSpace(UsuarioForm.USU_NOM) || string.IsNullOrWhiteSpace(UsuarioForm.USU_COR))
            {
                await JS.InvokeVoidAsync("alert", "Nombre y Correo son obligatorios.");
                return;
            }

            // 3. Duplicados
            var duplicado = Usuarios.FirstOrDefault(u =>
                (u.USU_COR.ToLower() == UsuarioForm.USU_COR.ToLower() || u.USU_NUM == UsuarioForm.USU_NUM)
                && u.USU_ID != UsuarioForm.USU_ID);

            if (duplicado != null)
            {
                await JS.InvokeVoidAsync("alert", "El Correo o Número de empleado ya existen.");
                return;
            }

            if (!Editando) UsuarioForm.USU_CON = PasswordCheck;

            await GuardarCambios();
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
                    Usu_CON = UsuarioForm.USU_CON,
                    Usu_TEL = UsuarioForm.USU_TEL,
                    Usu_EST = UsuarioForm.USU_EST,
                    Rol_ID = UsuarioForm.ROL_ID, // Se mantiene el que ya tenga o nulo para asignar después
                    Are_ID = UsuarioForm.ARE_ID,
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
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        private async Task EliminarUsuario(Guid id)
        {
            var user = Usuarios.FirstOrDefault(u => u.USU_ID == id);
            if (await JS.InvokeAsync<bool>("confirm", $"¿Desea dar de baja (desactivar) a {user?.USU_NOM}?"))
            {
                // En lugar de llamar a DeleteUsuario, llamamos a Update para cambiar el estado
                var input = new UsuarioModelInput
                {
                    Usu_ID = user.USU_ID,
                    Usu_NOM = user.USU_NOM,
                    Usu_NUM = user.USU_NUM,
                    Usu_COR = user.USU_COR,
                    Usu_EST = false, // <--- Aquí lo desactivamos
                    Usu_FEC_MOD = DateTimeOffset.Now
                };

                await Client.UpdateUsuario.ExecuteAsync(input);
                await CargarDatos();
            }
        }
    }
}