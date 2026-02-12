using AP.BTP.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace AP.BTP.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChatController : ControllerBase
    {
        private const string SystemPrompt = "Je bent de BTP assistent voor Boomkwekerij Taak Planner (interne tool voor boomkwekerij medewerkers en admins). Beantwoord alleen vragen over deze app. Leg per onderwerp uit waar het te vinden is (Dashboard/Kalender/Herkenning/Instellingen/Chat/footer) en hoe je het gebruikt, in 1-2 korte zinnen. In Herkenning kunnen alleen foto’s worden geüpload (geen live camera). Als je het niet weet, zeg: 'Ik weet het niet, geef meer details.'";
        private readonly GeminiChatService _geminiChat;
        private readonly TaskContextService _taskContext;
        private readonly DefaultUserStore _defaultUserStore;
        private readonly ILogger<ChatController> _logger;

        public ChatController(GeminiChatService geminiChat, TaskContextService taskContext, DefaultUserStore defaultUserStore, ILogger<ChatController> logger)
        {
            _geminiChat = geminiChat;
            _taskContext = taskContext;
            _defaultUserStore = defaultUserStore;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            string prompt;
            string? userEmail = null;
            string? authId = null;
            int? userId = null;
            DateTime? weekDate = null;
            bool useNextWeek = false;

            try
            {
                var raw = await ReadRequestBodyAsync();
                using var doc = JsonDocument.Parse(raw);

                prompt = ExtractPrompt(doc);
                ExtractBasicFields(doc, ref userEmail, ref authId, ref userId, ref useNextWeek, ref weekDate);
                ApplyNextWeekFallback(ref useNextWeek, ref weekDate);

                ExtractFromClaims(ref userEmail, ref authId, ref userId);
                ExtractFromHeaders(Request.Headers, ref userEmail, ref authId, ref userId);
                ExtractFromQuery(HttpContext.Request, ref userEmail, ref authId, ref userId);

                ApplyDefaultUserFallback(ref userEmail, ref authId, ref userId);

                LogUserResolution(userEmail, authId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ongeldig body-formaat voor chat.");
                return BadRequest(new { error = "Ongeldige aanvraag" });
            }

            try
            {
                var context = await _taskContext.GetWeeklyTaskSummaryAsync(User, userEmail, authId, userId, weekDate, HttpContext.RequestAborted);
                var responseText = await _geminiChat.GenerateAsync(SystemPrompt, prompt, context, HttpContext.RequestAborted);
                return Ok(new { response = responseText });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("takencontext", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(ex, "Gebruiker ontbreekt voor takencontext.");
                    return StatusCode(StatusCodes.Status400BadRequest, new { error = "Geen gebruiker gevonden; geef 'userEmail' of 'userId' mee, of log in." });
                }

                _logger.LogError(ex, "Gemini configuratie ontbreekt of is ongeldig.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Gemini API niet geconfigureerd" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatfout bij prompt.");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Chatservice niet bereikbaar" });
            }
        }

        #region Helper

        private const string NullLiteral = "<null>";

        private async Task<string> ReadRequestBodyAsync()
        {
            if (Request.ContentLength is 0)
                throw new InvalidOperationException("Body is leeg; stuur JSON met minimaal 'prompt' mee.");

            using var reader = new StreamReader(Request.Body);
            var raw = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Body is leeg; stuur JSON met minimaal 'prompt' mee.");

            return raw;
        }

        private static string ExtractPrompt(JsonDocument doc)
        {
            if (!doc.RootElement.TryGetProperty("prompt", out var promptElement))
                throw new InvalidOperationException("prompt ontbreekt");

            var prompt = promptElement.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(prompt))
                throw new InvalidOperationException("prompt mag niet leeg zijn");

            return prompt;
        }

        private static void ExtractBasicFields(
            JsonDocument doc,
            ref string? userEmail,
            ref string? authId,
            ref int? userId,
            ref bool useNextWeek,
            ref DateTime? weekDate)
        {
            if (doc.RootElement.TryGetProperty("userEmail", out var emailElement))
                userEmail = emailElement.GetString();

            if (doc.RootElement.TryGetProperty("authId", out var authIdElement))
                authId = authIdElement.GetString();

            ExtractUserId(doc, ref userId);
            ExtractNextWeek(doc, ref useNextWeek);
            ExtractWeekDate(doc, ref weekDate);
        }

        private static void ExtractUserId(JsonDocument doc, ref int? userId)
        {
            if (!doc.RootElement.TryGetProperty("userId", out var userIdElement))
                return;

            if (int.TryParse(userIdElement.GetRawText(), out var parsedId))
                userId = parsedId;
            else if (userIdElement.ValueKind == JsonValueKind.String &&
                     int.TryParse(userIdElement.GetString(), out parsedId))
                userId = parsedId;
        }

        private static void ExtractNextWeek(JsonDocument doc, ref bool useNextWeek)
        {
            if (doc.RootElement.TryGetProperty("nextWeek", out var nextWeekElement))
            {
                useNextWeek =
                    nextWeekElement.ValueKind == JsonValueKind.True ||
                    (nextWeekElement.ValueKind == JsonValueKind.String &&
                     bool.TryParse(nextWeekElement.GetString(), out var parsed) && parsed);
            }
        }

        private static void ExtractWeekDate(JsonDocument doc, ref DateTime? weekDate)
        {
            if (doc.RootElement.TryGetProperty("weekDate", out var weekDateElement) 
                && DateTime.TryParse(
                weekDateElement.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
            {
                weekDate = parsed.Date;
            }
        }

        private static void ApplyNextWeekFallback(ref bool useNextWeek, ref DateTime? weekDate)
        {
            if (useNextWeek && weekDate == null)
                weekDate = DateTime.Today.AddDays(7);
        }

        private void ExtractFromClaims(ref string? userEmail, ref string? authId, ref int? userId)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                userEmail = User?.Claims?.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

            if (string.IsNullOrWhiteSpace(authId))
                authId = User?.Claims?.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

            if (!userId.HasValue)
            {
                var idClaim = User?.Claims?.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == "user_id" ||
                    c.Type == ClaimTypes.Name);

                if (idClaim != null && int.TryParse(idClaim.Value, out var parsedId))
                    userId = parsedId;
            }
        }

        private static void ExtractFromHeaders(
            IHeaderDictionary headers,
            ref string? userEmail,
            ref string? authId,
            ref int? userId)
        {
            TryHeader(headers, "X-UserEmail", ref userEmail);
            TryHeader(headers, "UserEmail", ref userEmail);
            TryHeader(headers, "X-AuthId", ref authId);

            TryHeaderInt(headers, "X-UserId", ref userId);
            TryHeaderInt(headers, "UserId", ref userId);
        }


        private static void TryHeader(IHeaderDictionary headers, string key, ref string? target)
        {
            if (string.IsNullOrWhiteSpace(target) &&
                headers.TryGetValue(key, out var value))
            {
                target = value.FirstOrDefault();
            }
        }

        private static void TryHeaderInt(IHeaderDictionary headers, string key, ref int? target)
        {
            if (!target.HasValue &&
                headers.TryGetValue(key, out var value) &&
                int.TryParse(value.FirstOrDefault(), out var parsed))
            {
                target = parsed;
            }
        }

        private static void ExtractFromQuery(
            HttpRequest request,
            ref string? userEmail,
            ref string? authId,
            ref int? userId)
        {
            TryQuery(request, "userEmail", ref userEmail);
            TryQuery(request, "authId", ref authId);
            TryQueryInt(request, "userId", ref userId);
        }

        private static void TryQuery(HttpRequest req, string key, ref string? target)
        {
            if (string.IsNullOrWhiteSpace(target) &&
                req.Query.TryGetValue(key, out var value))
            {
                target = value.FirstOrDefault();
            }
        }

        private static void TryQueryInt(HttpRequest req, string key, ref int? target)
        {
            if (!target.HasValue &&
                req.Query.TryGetValue(key, out var value) &&
                int.TryParse(value.FirstOrDefault(), out var parsed))
            {
                target = parsed;
            }
        }

        private void ApplyDefaultUserFallback(ref string? userEmail, ref string? authId, ref int? userId)
        {
            if (!string.IsNullOrWhiteSpace(userEmail) ||
                !string.IsNullOrWhiteSpace(authId) ||
                userId.HasValue)
                return;

            userEmail = _defaultUserStore.Email;
            authId = _defaultUserStore.AuthId;
            userId = _defaultUserStore.UserId;

            if (!string.IsNullOrWhiteSpace(userEmail) ||
                !string.IsNullOrWhiteSpace(authId) ||
                userId.HasValue)
            {
                _logger.LogInformation("Fallback DefaultUser gebruikt uit DefaultUserStore.");
            }
        }

        private void LogUserResolution(string? userEmail, string? authId, int? userId)
        {
            var claimEmail = User?.Claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.Email || c.Type == "email")?.Value ?? NullLiteral;

            _logger.LogInformation(
                "Chat request user resolve: userEmail={UserEmail}, authId={AuthId}, userId={UserId}, claimEmail={ClaimEmail}",
                userEmail ?? NullLiteral,
                authId ?? NullLiteral,
                userId?.ToString() ?? NullLiteral,
                claimEmail);
        }

        #endregion
    }
}
