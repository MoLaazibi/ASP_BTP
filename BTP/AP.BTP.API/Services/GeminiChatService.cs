using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace AP.BTP.API.Services
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-2.5-flash";
        public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1";
    }

    public class GeminiChatService
    {
        private readonly GeminiOptions _options;
        private readonly Client _client;
        private readonly ILogger<GeminiChatService> _logger;
        private const int MaxContextChars = 800;

        public GeminiChatService(IOptions<GeminiOptions> options, ILogger<GeminiChatService> logger)
        {
            _options = options.Value;
            _logger = logger;
            
            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _client = new Client(vertexAI: false, apiKey: _options.ApiKey);
            }
            else
            {
                _logger.LogWarning("Gemini API Key is missing in configuration.");
            }
        }

        public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, string? context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey) || _client == null)
            {
                throw new InvalidOperationException("Gemini API key ontbreekt.");
            }

            var config = new GenerateContentConfig
            {
                Temperature = 0.2,
                MaxOutputTokens = 2048,
                SystemInstruction = new Content
                {
                    Role = "system",
                    Parts = new List<Part> { new Part { Text = systemPrompt } }
                }
            };

            var userContent = new Content
            {
                Role = "user",
                Parts = BuildUserParts(userPrompt, context)
            };

            var response = await _client.Models.GenerateContentAsync(_options.Model, userContent, config);

            var candidate = response?.Candidates?.FirstOrDefault();
            var text = candidate?.Content?.Parts?
                .Where(p => !string.IsNullOrEmpty(p.Text))
                .Aggregate(new System.Text.StringBuilder(), (sb, p) => sb.Append(p.Text), sb => sb.ToString());

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var finish = candidate?.FinishReason?.ToString() ?? "onbekend";
            _logger.LogWarning("Gemini gaf geen content terug. FinishReason: {FinishReason}", finish);

            if (finish.Equals("MAX_TOKENS", StringComparison.OrdinalIgnoreCase))
            {
                return "(Antwoord te lang, probeer je vraag korter te stellen.)";
            }

            return $"(Geen antwoord van Gemini ontvangen; status: {finish})";
        }

        private static List<Part> BuildUserParts(string userPrompt, string? context)
        {
            var parts = new List<Part> { new Part { Text = userPrompt } };
            if (!string.IsNullOrWhiteSpace(context))
            {
                var safeContext = context.Length > MaxContextChars
                    ? context[..MaxContextChars] + "…"
                    : context;
                parts.Add(new Part { Text = $"Context:\n{safeContext}" });
            }
            return parts;
        }
    }
}
