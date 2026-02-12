using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace AP.BTP.UI.Handlers
{
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public BearerTokenHandler(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                var token = user?.FindFirst("access_token")?.Value;
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // ignore retrieval failures
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

