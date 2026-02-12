using AP.BTP.MobileUI.Services.Auth;
using System.Net.Http.Headers;

namespace AP.BTP.MobileUI.Handlers
{
    public class AuthorizationHeaderHandler : DelegatingHandler
    {
        private readonly MobileAuthenticationStateProvider _authProvider;

        public AuthorizationHeaderHandler(MobileAuthenticationStateProvider authProvider)
        {
            _authProvider = authProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authProvider.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var email = await _authProvider.GetEmailAsync();
            if (!string.IsNullOrWhiteSpace(email))
            {
                request.Headers.Add("X-UserEmail", email);
            }

            var userId = await _authProvider.GetUserIdAsync();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                request.Headers.Add("X-UserId", userId);
            }

            var authId = await _authProvider.GetAuthIdAsync();
            if (!string.IsNullOrWhiteSpace(authId))
            {
                request.Headers.Add("X-AuthId", authId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}