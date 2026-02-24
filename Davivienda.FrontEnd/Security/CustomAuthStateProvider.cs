using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.LocalStorage; // 🔥 CAMBIO: LocalStorage en lugar de SessionStorage
using System.IdentityModel.Tokens.Jwt;

namespace Davivienda.FrontEnd.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage; // 🔥 CAMBIO
        private const int INACTIVITY_HOURS = 5; // 🔥 Tiempo de inactividad permitido

        public CustomAuthStateProvider(ILocalStorageService localStorage) // 🔥 CAMBIO
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return Anonymous();
                }

                // 🔥 VALIDAR TIEMPO DE ÚLTIMA ACTIVIDAD
                var lastActivityStr = await _localStorage.GetItemAsync<string>("lastActivity");
                if (!string.IsNullOrEmpty(lastActivityStr))
                {
                    if (DateTime.TryParse(lastActivityStr, out DateTime lastActivity))
                    {
                        var hoursSinceLastActivity = (DateTime.UtcNow - lastActivity).TotalHours;

                        if (hoursSinceLastActivity > INACTIVITY_HOURS)
                        {
                            // Si pasaron más de 5 horas, cerrar sesión
                            await _localStorage.RemoveItemAsync("authToken");
                            await _localStorage.RemoveItemAsync("lastActivity");
                            Console.WriteLine($"⏰ Sesión expirada por inactividad ({hoursSinceLastActivity:F2} horas)");
                            return Anonymous();
                        }
                    }
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Validar fecha de expiración del token
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    await _localStorage.RemoveItemAsync("authToken");
                    await _localStorage.RemoveItemAsync("lastActivity");
                    Console.WriteLine("⏰ Token JWT expirado");
                    return Anonymous();
                }

                // 🔥 ACTUALIZAR ÚLTIMA ACTIVIDAD
                await _localStorage.SetItemAsync("lastActivity", DateTime.UtcNow.ToString("o"));

                var claims = new List<Claim>();

                foreach (var claim in jwtToken.Claims)
                {
                    if (claim.Type == "role" ||
                        claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                        claim.Type == ClaimTypes.Role)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    }
                    else if (claim.Type == "unique_name" ||
                             claim.Type == "name" ||
                             claim.Type == ClaimTypes.Name ||
                             claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    {
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    }
                    else
                    {
                        claims.Add(claim);
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetAuthenticationStateAsync: {ex.Message}");
                return Anonymous();
            }
        }

        public async Task NotifyLogin(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = new List<Claim>();

            foreach (var claim in jwtToken.Claims)
            {
                if (claim.Type == "role" ||
                    claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                    claim.Type == ClaimTypes.Role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                }
                else if (claim.Type == "unique_name" ||
                         claim.Type == "name" ||
                         claim.Type == ClaimTypes.Name ||
                         claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                {
                    claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                }
                else
                {
                    claims.Add(claim);
                }
            }

            // 🔥 GUARDAR ÚLTIMA ACTIVIDAD AL HACER LOGIN
            await _localStorage.SetItemAsync("lastActivity", DateTime.UtcNow.ToString("o"));

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(user));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task NotifyLogout()
        {
            // 🔥 LIMPIAR TODO AL CERRAR SESIÓN
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("lastActivity");

            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private AuthenticationState Anonymous()
            => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}