using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.LocalStorage; // Asegúrate de tener este using

namespace Davivienda.FrontEnd.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Aquí normalmente decodificarías el JWT, por ahora seguimos con la simulación
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Usuario Davivienda"),
                new Claim(ClaimTypes.Role, "LiderTecnico"),
            }, "ServerAuth");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // MÉTODO PARA NOTIFICAR EL CIERRE DE SESIÓN
        public void NotifyLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }
    }
}