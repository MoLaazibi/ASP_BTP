using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AP.BTP.UI.Services.Auth
{
    public class TokenValidationService
    {
        public async Task<ClaimsPrincipal> CreatePrincipalAsync(IConfiguration config, string idToken, List<AP.BTP.Domain.Role>? roles)
        {
            var domain = config["Auth0:Domain"];
            var clientId = config["Auth0:ClientId"];
            var endpoint = $"https://{domain}/.well-known/openid-configuration";

            var manager = new ConfigurationManager<OpenIdConnectConfiguration>(endpoint, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever());
            var discovery = await manager.GetConfigurationAsync(CancellationToken.None);

            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{domain}/",
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = discovery.SigningKeys
            };

            var principal = handler.ValidateToken(idToken, parameters, out _);
            if (roles != null && roles.Any())
            {
                var identity = (ClaimsIdentity)principal.Identity!;
                foreach (var role in roles)
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
            }
            return principal;
        }
    }
}
