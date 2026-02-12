using AP.BTP.API.Services;
using AP.BTP.Application.CQRS.Users;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AP.BTP.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        private readonly DefaultUserStore _defaultUserStore;

        public AuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IMemoryCache cache, IUserRepository userRepository, IMediator mediator, ILogger<AuthController> logger, DefaultUserStore defaultUserStore)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _userRepository = userRepository;
            _mediator = mediator;
            _logger = logger;
            _defaultUserStore = defaultUserStore;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Email en wachtwoord is verplicht." });

            var response = await AuthenticateWithAuth0(req.Email, req.Password);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return BadRequest(new { message = err });
            }

            var token = await response.Content.ReadFromJsonAsync<Auth0TokenResponse>();
            if (token == null || string.IsNullOrEmpty(token.IdToken))
            {
                return BadRequest(new { message = "Er is iets fout gegaan tijdens het inloggen met Auth0." });
            }
            var principal = await CreatePrincipalFromIdTokenAsync(token.IdToken);

            // Sign in to create the authentication cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            var roles = principal.FindAll(ClaimTypes.Role)
                .Select(c => Enum.TryParse<Role>(c.Value, true, out var r) ? r : Role.NietApproved)
                .ToList();

            var loginEmail = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
            var loginAuthId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
            int? loginUserId = null;
            var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("user_id") ?? principal.FindFirstValue(ClaimTypes.Name);
            if (int.TryParse(idClaim, out var parsedLoginId))
            {
                loginUserId = parsedLoginId;
            }
            _defaultUserStore.Set(loginEmail, loginAuthId, loginUserId);

            return Ok(new LoginResult(
                new UserDto(
                    principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                    principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                    principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                    principal.FindFirstValue("picture") ?? string.Empty,
                    roles
                ),
                token.IdToken,
                token.AccessToken
            ));
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var validationError = ValidateRegisterRequest(req);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            if (await _userRepository.GetByEmail(req.Email) != null)
                return BadRequest(new { message = "Email word al gebruikt door een account." });

            var auth0Response = await RegisterWithAuth0(req.Email, req.Username, req.Password, req.FirstName, req.LastName);
            if (!auth0Response.IsSuccessStatusCode)
            {
                return await HandleAuth0ErrorResponse(auth0Response);
            }

            var signupResponse = await ParseAuth0SignupResponse(auth0Response, req.Email);
            if (signupResponse == null)
                return BadRequest(new { message = "Failed to get user ID from Auth0." });

            var addUserResult = await AddUserToDatabase(req, signupResponse);
            if (!addUserResult.Success)
                return BadRequest(new { message = addUserResult.Message ?? "Failed to add user to database." });

            return Ok(new { message = "Registratie gelukt." });
        }

        private static string? ValidateRegisterRequest(RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.Username))
                return "Email, gebruikersnaam en wachtwoord is verplicht.";

            if (req.Password != req.ConfirmPassword)
                return "Wachtwoorden komen niet overeen.";

            return null;
        }

        private async Task<IActionResult> HandleAuth0ErrorResponse(HttpResponseMessage auth0Response)
        {
            var errorContent = await auth0Response.Content.ReadAsStringAsync();
            _logger.LogWarning("Auth0 signup failed with status {StatusCode}: {ErrorContent}",
                auth0Response.StatusCode, errorContent);

            string errorMessage = "Fout tijdens registratie. Neem contact op met een administrator.";

            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorObj.TryGetProperty("description", out var desc)) errorMessage = desc.GetString() ?? errorMessage;
                else if (errorObj.TryGetProperty("message", out var msg)) errorMessage = msg.GetString() ?? errorMessage;
                else if (errorObj.TryGetProperty("error_description", out var errDesc)) errorMessage = errDesc.GetString() ?? errorMessage;
                else if (errorObj.TryGetProperty("error", out var err)) errorMessage = err.GetString() ?? errorMessage;
            }
            catch
            {
                errorMessage = errorContent;
            }

            return BadRequest(new { message = errorMessage });
        }

        private async Task<Auth0SignupResponse?> ParseAuth0SignupResponse(HttpResponseMessage response, string email)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<Auth0SignupResponse>(content);
            }
            catch
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<JsonElement>(content);
                    if (obj.TryGetProperty("_id", out var id))
                        return new Auth0SignupResponse { Id = id.GetString() ?? string.Empty, Email = email };
                    if (obj.TryGetProperty("user_id", out var userId))
                        return new Auth0SignupResponse { Id = userId.GetString() ?? string.Empty, Email = email };
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private async Task<(bool Success, string? Message)> AddUserToDatabase(RegisterRequest req, Auth0SignupResponse signupResponse)
        {
            var command = new AddUserCommand
            {
                User = new UserDTO
                {
                    AuthId = signupResponse.Id,
                    Email = req.Email,
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Username = req.Username,
                    Roles = new List<Role>()
                }
            };

            var result = await _mediator.Send(command);
            return (result.Data != null, result.Message);
        }

        private async Task<HttpResponseMessage> RegisterWithAuth0(string email, string username, string password, string firstName, string lastName)
        {
            var domain = _configuration["Auth0:Domain"];
            var clientId = _configuration["Auth0:ClientId"];
            var connection = "Username-Password-Authentication";

            if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(clientId))
            {
                throw new InvalidOperationException("Auth0 configuration is missing.");
            }

            var payload = new
            {
                client_id = clientId,
                email = email,
                username = username,
                password = password,
                connection = connection,
                user_metadata = new { first_name = firstName, last_name = lastName }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{domain}/dbconnections/signup")
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            return response;
        }


        private async Task<HttpResponseMessage> AuthenticateWithAuth0(string email, string password)
        {
            var domain = _configuration["Auth0:Domain"];
            var clientId = _configuration["Auth0:ClientId"];
            var clientSecret = _configuration["Auth0:ClientSecret"];
            var audience = _configuration["Auth0:Audience"];

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "http://auth0.com/oauth/grant-type/password-realm",
                ["username"] = email,
                ["password"] = password,
                ["client_id"] = clientId ?? string.Empty,
                ["client_secret"] = clientSecret ?? string.Empty,
                ["audience"] = audience ?? string.Empty,
                ["scope"] = "openid profile email",
                ["realm"] = "Username-Password-Authentication"
            };

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{domain}/oauth/token")
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            return await client.SendAsync(request);
        }

        private async Task<ClaimsPrincipal> CreatePrincipalFromIdTokenAsync(string idToken)
        {
            var domain = _configuration["Auth0:Domain"];
            var clientId = _configuration["Auth0:ClientId"];
            var wellKnownEndpoint = $"https://{domain}/.well-known/openid-configuration";

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownEndpoint,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            var config = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            var handler = new JwtSecurityTokenHandler();

            var validation = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{domain}/",
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = config.SigningKeys
            };

            var principal = handler.ValidateToken(idToken, validation, out _);

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return principal;
            }

            var user = await _userRepository.GetByEmail(email);
            if (user != null)
            {
                var identity = (ClaimsIdentity)principal.Identity!;
                foreach (var role in user.Roles.Select(ur => ur.Role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                }
            }
            return principal;
        }


        public record LoginRequest(string Email, string Password);
        public record RegisterRequest(string Email, string FirstName, string LastName, string Username, string Password, string ConfirmPassword);

        public class Auth0TokenResponse
        {
            [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
            [JsonPropertyName("id_token")] public string IdToken { get; set; } = string.Empty;
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
            [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;
        }

        public class Auth0SignupResponse
        {
            [JsonPropertyName("_id")] public string Id { get; set; } = string.Empty;
            [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        }


        public record LoginResult(UserDto User, string Token, string AccessToken);
        public record UserDto(string Id, string Email, string? Name, string? Picture, List<Role> Roles);
    }
}
