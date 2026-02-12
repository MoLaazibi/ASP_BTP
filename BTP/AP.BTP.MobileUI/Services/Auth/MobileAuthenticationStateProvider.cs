using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace AP.BTP.MobileUI.Services.Auth
{
    public class MobileAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public MobileAuthenticationStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "btp_access_token");
            var email = await _js.InvokeAsync<string>("localStorage.getItem", "btp_user_email");
            var name = await _js.InvokeAsync<string>("localStorage.getItem", "btp_user_name");
            var rolesCsv = await _js.InvokeAsync<string>("localStorage.getItem", "btp_user_roles");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var identity = new ClaimsIdentity("btp-auth");

            if (!string.IsNullOrWhiteSpace(email))
                identity.AddClaim(new Claim(ClaimTypes.Name, email));
            if (!string.IsNullOrWhiteSpace(name))
                identity.AddClaim(new Claim(ClaimTypes.GivenName, name));
            identity.AddClaim(new Claim("access_token", token));

            if (!string.IsNullOrWhiteSpace(rolesCsv))
            {
                foreach (var role in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }

        public async Task SetUserAsync(ApiUser user, string token)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "btp_access_token", token);
            await _js.InvokeVoidAsync("localStorage.setItem", "btp_user_email", user.Email);
            await _js.InvokeVoidAsync("localStorage.setItem", "btp_user_name", user.Name ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", "btp_auth_id", user.Id);
            var rolesCsv = user.Roles != null ? string.Join(',', user.Roles.Select(r => r.ToString())) : string.Empty;
            await _js.InvokeVoidAsync("localStorage.setItem", "btp_user_roles", rolesCsv);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<string?> GetTokenAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "btp_access_token");
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }

        public async Task<string?> GetUserIdAsync()
        {
            var id = await _js.InvokeAsync<string>("localStorage.getItem", "btp_user_id");
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        public async Task<string?> GetAuthIdAsync()
        {
            var id = await _js.InvokeAsync<string>("localStorage.getItem", "btp_auth_id");
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        public async Task<string?> GetEmailAsync()
        {
            var email = await _js.InvokeAsync<string>("localStorage.getItem", "btp_user_email");
            return string.IsNullOrWhiteSpace(email) ? null : email;
        }

        public async Task<List<string>> GetRolesAsync()
        {
            var rolesCsv = await _js.InvokeAsync<string>("localStorage.getItem", "btp_user_roles");
            return string.IsNullOrWhiteSpace(rolesCsv) ? new List<string>() : rolesCsv.Split(',').Select(r => r.Trim()).Where(r => r.Length > 0).ToList();
        }

        public async Task SignOutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "btp_access_token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "btp_user_email");
            await _js.InvokeVoidAsync("localStorage.removeItem", "btp_user_name");
            await _js.InvokeVoidAsync("localStorage.removeItem", "btp_user_roles");
            await _js.InvokeVoidAsync("localStorage.removeItem", "btp_user_id");
            await _js.InvokeVoidAsync("localStorage.removeItem", "btp_auth_id");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
