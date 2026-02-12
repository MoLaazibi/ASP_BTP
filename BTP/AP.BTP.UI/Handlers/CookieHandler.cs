namespace AP.BTP.UI.Handlers
{
    public class CookieHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CookieHandler>? _logger;

        public CookieHandler(IServiceProvider serviceProvider, ILoggerFactory? loggerFactory = null)
        {
            _serviceProvider = serviceProvider;
            _logger = loggerFactory?.CreateLogger<CookieHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext;

                // Stuur alle cookies van de browser door, niet alleen de auth-cookie.
                var cookieHeader = httpContext?.Request.Headers.Cookie.FirstOrDefault();

                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    request.Headers.Add("Cookie", cookieHeader);
                    _logger?.LogDebug("Forwarding cookies to {Url}: {CookieHeader}",
                        request.RequestUri, cookieHeader.Substring(0, Math.Min(50, cookieHeader.Length)));
                }
                else
                {
                    _logger?.LogWarning("No cookies found in request to {Url}", request.RequestUri);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}