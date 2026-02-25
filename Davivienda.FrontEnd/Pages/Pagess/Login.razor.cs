using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
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
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private GqlSdk.LoginInput loginModel = new GqlSdk.LoginInput();
        private string errorMsg = "";
        private bool cargando = false;
        private bool mostrarRecuperar = false;
        private string correoRecuperar = "";

        protected override async Task OnInitializedAsync()
        {
            // 🔥 VERIFICAR SI YA HAY SESIÓN ACTIVA
            var token = await LocalStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();

                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine("✅ Sesión activa encontrada, redirigiendo...");
                    Nav.NavigateTo("/home");
                }
                else
                {
                    // Token inválido o expirado
                    Console.WriteLine("⚠️ Token inválido, limpiando...");
                    await LocalStorage.RemoveItemAsync("authToken");
                }
            }
        }

        private async Task ProcesarLogin()
        {
            cargando = true;
            errorMsg = "";

            try
            {
                // 🔥 LIMPIAR TOKEN ANTERIOR SI EXISTE
                await LocalStorage.RemoveItemAsync("authToken");

                Console.WriteLine("🔐 Solicitando nuevo token al backend...");

                var result = await Client.IniciarSesion.ExecuteAsync(loginModel);

                if (result.Data?.Login.Exito == true)
                {
                    string? token = result.Data.Login.Token;

                    if (!string.IsNullOrEmpty(token))
                    {
                        // 🔥 GUARDAR NUEVO TOKEN
                        await LocalStorage.SetItemAsync("authToken", token);

                        Console.WriteLine("✅ Nuevo token guardado en LocalStorage");

                        if (AuthStateProvider is CustomAuthStateProvider customAuth)
                        {
                            await customAuth.NotifyLogin(token);
                        }

                        Nav.NavigateTo("/home");
                    }
                    else
                    {
                        errorMsg = "Error interno: El servidor no proporcionó un token válido.";
                    }
                }
                else
                {
                    errorMsg = result.Data?.Login.Mensaje ?? "Credenciales inválidas";
                }
            }
            catch (Exception ex)
            {
                errorMsg = "No se pudo establecer conexión con el servidor.";
                Console.WriteLine($"❌ Login Error: {ex.Message}");
            }
            finally
            {
                cargando = false;
                StateHasChanged();
            }
        }

        private void EnviarCorreo()
        {
            mostrarRecuperar = false;
        }
    }
}