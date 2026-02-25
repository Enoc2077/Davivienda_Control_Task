using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;

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
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("❌ No hay token en LocalStorage");
                    return Anonymous();
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // 🔥 SOLO VALIDAR EXPIRACIÓN DEL TOKEN (6 horas desde que se creó)
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    Console.WriteLine($"⏰ Token expirado: {jwtToken.ValidTo} < {DateTime.UtcNow}");
                    await _localStorage.RemoveItemAsync("authToken");
                    return Anonymous();
                }

                Console.WriteLine($"✅ Token válido hasta: {jwtToken.ValidTo}");

                // Mapear claims
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

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(user));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task NotifyLogout()
        {
            await _localStorage.RemoveItemAsync("authToken");

            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private AuthenticationState Anonymous()
            => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}