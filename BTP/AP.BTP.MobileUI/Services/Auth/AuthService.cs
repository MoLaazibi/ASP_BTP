using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AP.BTP.MobileUI.Services.Auth
{
    public class AuthService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly MobileAuthenticationStateProvider _authProvider;
        private readonly NavigationManager _nav;

        public AuthService(IHttpClientFactory clientFactory, MobileAuthenticationStateProvider authProvider, NavigationManager nav)
        {
            _clientFactory = clientFactory;
            _authProvider = authProvider;
            _nav = nav;
        }

        public async Task<(bool ok, string? error)> LoginAsync(string email, string password)
        {
            var client = _clientFactory.CreateClient("API");
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();
                var message = FormatErrorMessage(raw);
                return (false, message);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiLoginResult>();
            if (result == null || result.User == null || string.IsNullOrWhiteSpace(result.AccessToken))
                return (false, "Ongeldige login respons");

            await _authProvider.SetUserAsync(result.User, result.AccessToken);

            var roles = result.User.Roles ?? new List<AP.BTP.Domain.Role>();
            var canAccessDashboard = roles.Contains(AP.BTP.Domain.Role.Admin) || roles.Contains(AP.BTP.Domain.Role.Medewerker);
            if (canAccessDashboard)
            {
                _nav.NavigateTo("", true);
            }
            else if (roles.Count == 0)
            {
                _nav.NavigateTo("login-error", true);
            }
            else if (roles.Contains(AP.BTP.Domain.Role.Verantwoordelijke) && !canAccessDashboard)
            {
                _nav.NavigateTo("use-desktop", true);
            }
            else
            {
                _nav.NavigateTo("login-error", true);
            }

            return (true, null);
        }

        public async Task AttachTokenAsync(HttpClient client)
        {
            var token = await _authProvider.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        static string FormatErrorMessage(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Login mislukt. Probeer het opnieuw.";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var msgProp))
                    {
                        var msg = msgProp.GetString();
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            // Sometimes message itself is a JSON string from Auth0
                            try
                            {
                                using var inner = System.Text.Json.JsonDocument.Parse(msg);
                                var innerRoot = inner.RootElement;
                                if (innerRoot.TryGetProperty("error_description", out var ed))
                                    return ed.GetString() ?? msg;
                                if (innerRoot.TryGetProperty("message", out var m2))
                                    return m2.GetString() ?? msg;
                            }
                            catch { }
                            return msg;
                        }
                    }
                    if (root.TryGetProperty("error_description", out var descProp))
                    {
                        var desc = descProp.GetString();
                        if (!string.IsNullOrWhiteSpace(desc)) return desc!;
                    }
                    if (root.TryGetProperty("error", out var errProp))
                    {
                        var err = errProp.GetString();
                        if (!string.IsNullOrWhiteSpace(err)) return err!;
                    }
                }
            }
            catch { }
            return "Login mislukt. Controleer je email of wachtwoord.";
        }
    }
}
