using Microsoft.AspNetCore.Components;
using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using GqlSdk = Davivienda.GraphQL.SDK;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Login
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

        private GqlSdk.LoginInput loginModel = new GqlSdk.LoginInput();

        private string errorMsg = "";
        private bool cargando = false;
        private bool mostrarRecuperar = false;
        private string correoRecuperar = "";

        // NUEVO: Verificación automática al cargar la página
        protected override async Task OnInitializedAsync()
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                // Si ya hay token, saltamos directamente al Home
                Nav.NavigateTo("/home");
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
                    string token = result.Data.Login.Token;
                    // Guardamos el token para persistencia
                    await LocalStorage.SetItemAsync("authToken", token);
                    Nav.NavigateTo("/home");
                }
                else
                {
                    errorMsg = result.Data?.Login.Mensaje ?? "Error: Credenciales inválidas.";
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
            mostrarRecuperar = false;
        }
    }
}