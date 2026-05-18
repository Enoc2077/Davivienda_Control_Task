using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Davivienda.FrontEnd.Security;
using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using System;
using System.Linq;
using System.Threading.Tasks;
using GqlSdk = Davivienda.GraphQL.SDK;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Login
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        // LOGIN
        private GqlSdk.LoginInput loginModel = new GqlSdk.LoginInput();
        private string errorMsg = "";
        private bool cargando = false;

        // REGISTRO
        private bool mostrarRegistro = false;
        private UsuarioModel nuevoUsuario = new UsuarioModel();
        private string errorRegistro = "";
        private string exitoRegistro = "";
        private bool registrando = false;
        private bool errorPassword = false;

        protected override async Task OnInitializedAsync()
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    Nav.NavigateTo("/home");
                }
                else
                {
                    await LocalStorage.RemoveItemAsync("authToken");
                }
            }
        }

        private async Task ProcesarLogin()
        {
            cargando = true;
            errorMsg = "";

            // --- Validaciones de caja negra: Número de empleado ---
            if (string.IsNullOrWhiteSpace(loginModel.UsuNum))
            {
                errorMsg = "El número de empleado es obligatorio.";
                cargando = false;
                return;
            }

            if (loginModel.UsuNum.Length != 5)
            {
                errorMsg = "El número de empleado debe tener exactamente 5 dígitos.";
                cargando = false;
                return;
            }

            if (!loginModel.UsuNum.All(char.IsDigit))
            {
                errorMsg = "El número de empleado solo debe contener números.";
                cargando = false;
                return;
            }

            // --- Validaciones de caja negra: Contraseña ---
            if (string.IsNullOrWhiteSpace(loginModel.Password))
            {
                errorMsg = "La contraseña es obligatoria.";
                cargando = false;
                return;
            }

            if (loginModel.Password.Length < 2)
            {
                errorMsg = "La contraseña debe tener al menos 2 caracteres.";
                cargando = false;
                return;
            }

            if (loginModel.Password.Length > 20)
            {
                errorMsg = "La contraseña no puede superar los 20 caracteres.";
                cargando = false;
                return;
            }

            try
            {
                await LocalStorage.RemoveItemAsync("authToken");

                var result = await Client.IniciarSesion.ExecuteAsync(loginModel);

                if (result.Data?.Login.Exito == true)
                {
                    string? token = result.Data.Login.Token;

                    if (!string.IsNullOrEmpty(token))
                    {
                        await LocalStorage.SetItemAsync("authToken", token);

                        if (AuthStateProvider is CustomAuthStateProvider customAuth)
                        {
                            await customAuth.NotifyLogin(token);
                        }

                        Nav.NavigateTo("/home");
                    }
                    else
                    {
                        errorMsg = "Error interno: el servidor no proporcionó un token válido.";
                    }
                }
                else
                {
                    errorMsg = result.Data?.Login.Mensaje ?? "Credenciales inválidas. Verifique su número de empleado y contraseña.";
                }
            }
            catch (Exception ex)
            {
                errorMsg = "No se pudo establecer conexión con el servidor.";
                Console.WriteLine($"Login Error: {ex.Message}");
            }
            finally
            {
                cargando = false;
                StateHasChanged();
            }
        }

        private async Task CrearUsuario()
        {
            registrando = true;
            errorRegistro = "";
            exitoRegistro = "";
            errorPassword = false;

            try
            {
                // --- Validaciones de caja negra: Número de empleado ---
                if (string.IsNullOrWhiteSpace(nuevoUsuario.USU_NUM))
                {
                    errorRegistro = "El número de empleado es obligatorio.";
                    registrando = false;
                    return;
                }

                if (nuevoUsuario.USU_NUM.Length != 5)
                {
                    errorRegistro = "El número de empleado debe tener exactamente 5 dígitos.";
                    registrando = false;
                    return;
                }

                if (!nuevoUsuario.USU_NUM.All(char.IsDigit))
                {
                    errorRegistro = "El número de empleado solo debe contener números.";
                    registrando = false;
                    return;
                }

                // --- Otras validaciones de registro ---
                if (string.IsNullOrWhiteSpace(nuevoUsuario.USU_NOM))
                {
                    errorRegistro = "El nombre completo es obligatorio.";
                    registrando = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(nuevoUsuario.USU_TEL))
                {
                    errorRegistro = "El teléfono es obligatorio.";
                    registrando = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(nuevoUsuario.USU_COR))
                {
                    errorRegistro = "El correo electrónico es obligatorio.";
                    registrando = false;
                    return;
                }

                // --- Validaciones de caja negra: Contraseña ---
                if (string.IsNullOrWhiteSpace(nuevoUsuario.USU_CON))
                {
                    errorPassword = true;
                    errorRegistro = "La contraseña es obligatoria.";
                    registrando = false;
                    return;
                }

                if (nuevoUsuario.USU_CON.Length < 2)
                {
                    errorPassword = true;
                    errorRegistro = "La contraseña debe tener al menos 2 caracteres.";
                    registrando = false;
                    return;
                }

                if (nuevoUsuario.USU_CON.Length > 20)
                {
                    errorPassword = true;
                    errorRegistro = "La contraseña no puede superar los 20 caracteres.";
                    registrando = false;
                    return;
                }

                var resRoles = await Client.GetRoles.ExecuteAsync();
                var rolServicio = resRoles.Data?.Roles?.FirstOrDefault(r =>
                    r.Rol_NOM.Equals("Servicio", StringComparison.OrdinalIgnoreCase));

                if (rolServicio == null)
                {
                    errorRegistro = "Error: no se encontró el rol 'Servicio' en el sistema.";
                    registrando = false;
                    return;
                }

                var rolServicioId = rolServicio.Rol_ID;

                nuevoUsuario.ROL_ID = rolServicioId;
                nuevoUsuario.USU_EST = true;
                nuevoUsuario.USU_FEC_CRE = DateTimeOffset.Now;

                var input = new GqlSdk.UsuarioModelInput
                {
                    Usu_ID = Guid.NewGuid(),
                    Usu_NUM = nuevoUsuario.USU_NUM,
                    Usu_NOM = nuevoUsuario.USU_NOM,
                    Usu_TEL = nuevoUsuario.USU_TEL,
                    Usu_COR = nuevoUsuario.USU_COR,
                    Usu_CON = nuevoUsuario.USU_CON,
                    Usu_EST = true,
                    Rol_ID = rolServicioId,
                    Usu_FEC_CRE = DateTimeOffset.Now
                };

                var result = await Client.InsertUsuario.ExecuteAsync(input);

                if (result.Data?.InsertUsuario != null)
                {
                    exitoRegistro = "Usuario creado exitosamente. Ahora puede iniciar sesión.";
                    await Task.Delay(2000);
                    CerrarModalRegistro();
                }
                else
                {
                    errorRegistro = "Error al crear el usuario. Intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                errorRegistro = $"Error: {ex.Message}";
                Console.WriteLine($"Excepción al crear usuario: {ex.Message}");
            }
            finally
            {
                registrando = false;
                StateHasChanged();
            }
        }

        private void CerrarModalRegistro()
        {
            mostrarRegistro = false;
            errorRegistro = "";
            exitoRegistro = "";
            errorPassword = false;
            nuevoUsuario = new UsuarioModel();
            StateHasChanged();
        }
    }
}