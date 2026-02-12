using AP.BTP.UI.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace AP.BTP.UI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/login", [IgnoreAntiforgeryToken] async (
                HttpContext httpContext,
                IHttpClientFactory clientFactory,
                IConfiguration config,
                ILogger<Program> logger,
                LoginRequest req,
                TokenValidationService tokenValidator) =>
            {
                var apiClient = clientFactory.CreateClient("API");
                var apiResponse = await apiClient.PostAsJsonAsync("/api/auth/login", req);
                if (!apiResponse.IsSuccessStatusCode)
                {
                    var errorContent = await apiResponse.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<Auth0ErrorResponse>(errorContent);
                    return Results.BadRequest(new { message = error?.ErrorDescription ?? "Invalid email or password." });
                }

                var loginResult = await apiResponse.Content.ReadFromJsonAsync<ApiLoginResult>();
                if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
                    return Results.BadRequest(new { message = "Invalid response from API." });

                var principal = await tokenValidator.CreatePrincipalAsync(config, loginResult.Token, loginResult.User?.Roles);
                if (!string.IsNullOrWhiteSpace(loginResult.AccessToken))
                {
                    var identity = (ClaimsIdentity)principal.Identity!;
                    identity.AddClaim(new Claim("access_token", loginResult.AccessToken));
                }

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(12) });

                return Results.Ok(new { message = "Login successful" });
            });

            app.MapGet("/logout", async (HttpContext httpContext, IHttpClientFactory clientFactory) =>
            {
                var apiClient = clientFactory.CreateClient("API");
                await apiClient.PostAsync("/api/auth/logout", null);
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                httpContext.Response.Redirect("/");
            });
        }
    }
}