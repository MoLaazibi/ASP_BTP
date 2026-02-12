using Microsoft.Extensions.Options;

namespace AP.BTP.API.Services
{
    public class DefaultUserOptions
    {
        public string? Email { get; set; }
        public string? AuthId { get; set; }
        public int? UserId { get; set; }
    }

    public class DefaultUserStore
    {
        private readonly ILogger<DefaultUserStore> _logger;

        public string? Email { get; private set; }
        public string? AuthId { get; private set; }
        public int? UserId { get; private set; }

        public DefaultUserStore(IOptions<DefaultUserOptions> options, ILogger<DefaultUserStore> logger)
        {
            _logger = logger;
            var opt = options.Value;
            Email = opt.Email;
            AuthId = opt.AuthId;
            UserId = opt.UserId;
        }

        public void Set(string? email, string? authId, int? userId)
        {
            Email = string.IsNullOrWhiteSpace(email) ? null : email;
            AuthId = string.IsNullOrWhiteSpace(authId) ? null : authId;
            UserId = userId;
            _logger.LogInformation("Default user updated to email={Email}, authId={AuthId}, userId={UserId}", Email ?? "<null>", AuthId ?? "<null>", UserId?.ToString() ?? "<null>");
        }
    }
}
