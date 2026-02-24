using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.SessionStorage;
using System.IdentityModel.Tokens.Jwt;

namespace Davivienda.FrontEnd.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorage;

        public CustomAuthStateProvider(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _sessionStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return Anonymous();
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    await _sessionStorage.RemoveItemAsync("authToken");
                    return Anonymous();
                }

                var claims = new List<Claim>();

                Console.WriteLine("========== CLAIMS EN EL TOKEN ==========");
                foreach (var claim in jwtToken.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");

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
                Console.WriteLine("========================================");

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetAuthenticationStateAsync: {ex.Message}");
                return Anonymous();
            }
        }

        public Task NotifyLogin(string token)
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
            return Task.CompletedTask;
        }

        public void NotifyLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private AuthenticationState Anonymous()
            => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}