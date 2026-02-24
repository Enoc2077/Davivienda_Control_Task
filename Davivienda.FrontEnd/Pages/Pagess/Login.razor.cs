using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization; // Necesario para el Provider
using Blazored.SessionStorage; // Cambiamos Local por Session
using Davivienda.FrontEnd.Security;
using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using System;
using System.Threading.Tasks;
using GqlSdk = Davivienda.GraphQL.SDK;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Login
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        // Inyectamos SessionStorage para el requisito de cerrar sesión al cerrar navegador
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;

        // Inyectamos nuestro proveedor de autenticación personalizado
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private GqlSdk.LoginInput loginModel = new GqlSdk.LoginInput();

        private string errorMsg = "";
        private bool cargando = false;
        private bool mostrarRecuperar = false;
        private string correoRecuperar = "";

        protected override async Task OnInitializedAsync()
        {
            var token = await SessionStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                // Validamos si el token sigue vigente antes de redirigir
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    Nav.NavigateTo("/home");
                }
                else
                {
                    // Si el token estaba ahí pero el Provider dice que no es válido (ej. pasaron las 6 horas),
                    // lo limpiamos para evitar basura en el almacenamiento.
                    await SessionStorage.RemoveItemAsync("authToken");
                }
            }
        }

        private async Task ProcesarLogin()
        {
            cargando = true;
            errorMsg = "";

            try
            {
                var result = await Client.IniciarSesion.ExecuteAsync(loginModel);

                if (result.Data?.Login.Exito == true)
                {
                    string? token = result.Data.Login.Token;

                    if (!string.IsNullOrEmpty(token))
                    {
                        await SessionStorage.SetItemAsync("authToken", token);

                        if (AuthStateProvider is CustomAuthStateProvider customAuth)
                        {
                            // Usamos await porque ahora NotifyLogin devuelve un Task (para evitar advertencia CS1998)
                            await customAuth.NotifyLogin(token);
                        }

                        Nav.NavigateTo("/home");
                    }
                    else
                    {
                        errorMsg = "Error interno: El servidor no proporcionó un token válido.";
                    }
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

        private void EnviarCorreo()
        {
            // Lógica para recuperar contraseña
            mostrarRecuperar = false;
        }
    }
}