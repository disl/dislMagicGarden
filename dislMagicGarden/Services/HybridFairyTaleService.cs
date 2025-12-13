using dislMagicGarden.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dislMagicGarden.Services
{
    public interface IHybridFairyTaleService
    {
        Task<FairyTaleResponse> GenerateFairyTaleAsync(FairyTaleRequest request);
        Task<decimal> EstimateCostAsync(FairyTaleRequest request);
        Task<TextOnlyResponse> GenerateTextOnlyAsync(string theme);
    }

    public class HybridFairyTaleService : IHybridFairyTaleService
    {
        private readonly HttpClient _httpClient;
        private readonly IConnectivity _connectivity;
        private readonly ILogger<HybridFairyTaleService> _logger;
        private readonly IConfiguration _configuration;

        // API Keys (aus SecureStorage oder appsettings)
        private readonly string _deepSeekApiKey;
        private readonly string _openAiApiKey;

        // DeepSeek Configuration
        private const string DEEPSEEK_API_URL = "https://api.deepseek.com/v1/chat/completions";
        private const string DEEPSEEK_MODEL = "deepseek-chat";

        // OpenAI Configuration
        private const string OPENAI_IMAGE_API_URL = "https://api.openai.com/v1/images/generations";

        // Preise (pro 1000 Tokens/Bild)
        private const decimal DEEPSEEK_INPUT_PRICE = 0.0000014M;   // $0.00014 pro 1K Tokens
        private const decimal DEEPSEEK_OUTPUT_PRICE = 0.0000028M;  // $0.00028 pro 1K Tokens
        private const decimal DALL_E_3_STANDARD_PRICE = 0.040M;    // $0.04 pro Bild
        private const decimal DALL_E_3_HD_PRICE = 0.080M;          // $0.08 pro Bild

        public HybridFairyTaleService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _connectivity = Connectivity.Current;
            _logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<HybridFairyTaleService>();

            _configuration = configuration;

            // API Keys aus Konfiguration laden
            _deepSeekApiKey = _configuration["DeepSeek:ApiKey"]
                ?? throw new ArgumentException("DeepSeek API Key fehlt. Bitte in appsettings.json eintragen.");

            _openAiApiKey = _configuration["OpenAI:ApiKey"]
                ?? throw new ArgumentException("OpenAI API Key fehlt. Bitte in appsettings.json eintragen.");

            // Timeout setzen
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<FairyTaleResponse> GenerateFairyTaleAsync(FairyTaleRequest request)
        {
            // Internetverbindung prüfen
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Keine Internetverbindung. Bitte überprüfe deine Verbindung.");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starte Märchen-Generierung für Thema: {Theme}", request.Theme);

                // 1. Text mit DeepSeek generieren
                var textResult = await GenerateTextWithDeepSeekAsync(request);

                var response = new FairyTaleResponse
                {
                    Title = textResult.Title,
                    Characters = textResult.Characters,
                    Story = textResult.Story,
                    Moral = textResult.Moral,
                    ImagePrompts = textResult.ImagePrompts,
                    Cost = new CostBreakdown
                    {
                        TextCost = textResult.EstimatedCost
                    }
                };

                // 2. Bilder nur bei FullStory Mode
                if (request.Mode == GenerationMode.FullStory)
                {
                    var images = await GenerateImagesWithOpenAIAsync(
                        response.ImagePrompts.Take(request.ImageCount).ToList(),
                        request.Style
                    );

                    response.ImageUrls = images;
                    response.Cost.ImageCost = CalculateOpenAIImageCost(images.Count, request.Style == "HD");
                }

                response.Cost.TotalCost = response.Cost.TextCost + response.Cost.ImageCost;
                response.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation("Märchen erfolgreich generiert: {Title} in {Time}ms",
                    response.Title, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Netzwerkfehler bei der Generierung");
                throw new Exception($"Netzwerkfehler: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON Parsing Fehler");
                throw new Exception("Fehler beim Verarbeiten der Antwort. Bitte versuche es erneut.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unerwarteter Fehler");
                throw new Exception($"Fehler: {ex.Message}");
            }
        }

        public async Task<TextOnlyResponse> GenerateTextOnlyAsync(string theme)
        {
            var request = new FairyTaleRequest
            {
                Theme = theme,
                Mode = GenerationMode.TextOnly
            };

            var result = await GenerateTextWithDeepSeekAsync(request);

            return new TextOnlyResponse
            {
                Title = result.Title,
                Story = result.Story,
                Moral = result.Moral,
                EstimatedCost = result.EstimatedCost
            };
        }

        public Task<decimal> EstimateCostAsync(FairyTaleRequest request)
        {
            decimal cost = 0;

            // Textkosten (geschätzt 800 Output-Tokens)
            cost += 800 * DEEPSEEK_OUTPUT_PRICE / 1000;

            // Bildkosten bei FullStory
            if (request.Mode == GenerationMode.FullStory)
            {
                var imagePrice = request.Style == "HD" ? DALL_E_3_HD_PRICE : DALL_E_3_STANDARD_PRICE;
                cost += request.ImageCount * imagePrice;
            }

            return Task.FromResult(cost);
        }

        // Private Hilfsmethoden
        private async Task<DeepSeekResult> GenerateTextWithDeepSeekAsync(FairyTaleRequest request)
        {
            var prompt = CreatePrompt(request);

            var requestBody = new
            {
                model = DEEPSEEK_MODEL,
                messages = new[]
                {
                    new { role = "system", content = "Du bist ein Märchenerzähler." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                //max_tokens = 1000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Authorization Header setzen
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _deepSeekApiKey);

            var response = await _httpClient.PostAsync(DEEPSEEK_API_URL, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DeepSeekApiResponse>(responseJson);

            if (apiResponse?.Choices?.FirstOrDefault()?.Message?.Content == null)
                throw new Exception("DeepSeek API gab keine gültige Antwort zurück.");

            var fairyTaleText = apiResponse.Choices[0].Message.Content;

            // Parse die Antwort
            return ParseFairyTaleResponse(fairyTaleText, apiResponse.Usage);
        }

        private async Task<List<string>> GenerateImagesWithOpenAIAsync(List<string> prompts, string style)
        {
            var images = new List<string>();

            foreach (var prompt in prompts)
            {
                try
                {
                    var imageUrl = await GenerateSingleImageAsync(prompt, style);
                    if (!string.IsNullOrEmpty(imageUrl))
                        images.Add(imageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fehler beim Generieren von Bild für Prompt: {Prompt}", prompt);
                    // Optional: Fallback-Bild oder Platzhalter
                }
            }

            return images;
        }

        private async Task<string> GenerateSingleImageAsync(string prompt, string style)
        {
            var requestBody = new
            {
                model = "dall-e-3",
                prompt = $"Children's book illustration, fairy tale style: {prompt}",
                n = 1,
                size = "1024x1024",
                quality = style == "HD" ? "hd" : "standard",
                style = style == "HD" ? "vivid" : "natural"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);

            var response = await client.PostAsync(OPENAI_IMAGE_API_URL, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var imageResponse = JsonSerializer.Deserialize<OpenAiImageResponse>(responseJson);

            return imageResponse?.Data?.FirstOrDefault()?.Url
                ?? throw new Exception("Keine Bild-URL erhalten");
        }

        private string CreatePrompt(FairyTaleRequest request)
        {
            //var styleText = request.Style switch
            //{
            //    "Classic" => "im klassischen Märchenstil",
            //    "Modern" => "im modernen, kindgerechten Stil",
            //    "Funny" => "im lustigen, humorvollen Stil",
            //    "HD" => "im sehr detaillierten Stil für hochwertige Illustrationen",
            //    _ => "im klassischen Märchenstil"
            //};

            var FairyTaleTypeText = request.FairyTaleType switch
            {
                FairyTaleType.Funny => "ein klassisches Märchen",
                FairyTaleType.Educational => "ein lehrreiches Märchen",
                FairyTaleType.Adventure => "ein abenteuerliches Märchen",
                FairyTaleType.Fantasy => "ein fantastisches Märchen",
                FairyTaleType.Modern => "ein modernes Märchen",
                FairyTaleType.Arabian => "ein arabisches Märchen",
                FairyTaleType.African => "ein afrikanisches Märchen",
                FairyTaleType.Asian => "ein asiatisches Märchen",
                FairyTaleType.Slavic => "ein slawisches Märchen",
                FairyTaleType.Perrault => "ein Märchen im Stil von Charles Perrault",
                FairyTaleType.Andersen => "ein Märchen im Stil von Hans Christian Andersen",
                FairyTaleType.Grimm => "ein Märchen im Stil der Brüder Grimm",
                _ => "ein klassisches Märchen"

            };


            string _currentLanguage = Thread.CurrentThread.CurrentCulture.NativeName;          

            return $$"""
                Erstelle ein komplettes (!) als {{FairyTaleTypeText}}. Ohne '...' am Ende. 
                
                Thema: {{request.Theme}}

                Ausgabesprache: {{_currentLanguage}}
                
                Format: JSON mit folgenden Feldern:
                {
                    "title": "Titel des Märchens",
                    "characters": ["Charakter 1", "Charakter 2", "Charakter 3"],
                    "story": "Die vollständige Geschichte (Dauer zirka {{ request.Duration_min }} min.)",
                    "moral": "Die Moral der Geschichte",
                    "image_prompts": [
                        "English description for image 1: magical forest scene",
                        "English description for image 2: main character meeting helper",
                        "English description for image 3: exciting adventure scene",
                        "English description for image 4: happy ending celebration"
                    ]
                }
                
                Die Bildbeschreibungen müssen auf Englisch sein und visuelle Details enthalten.
                """;
        }

        private DeepSeekResult ParseFairyTaleResponse(string responseText, ApiUsage usage)
        {
            try
            {
                // Entferne Markdown-Code-Blöcke
                var cleanText = responseText
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var result = JsonSerializer.Deserialize<DeepSeekResult>(cleanText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    throw new JsonException("Konnte JSON nicht parsen");

                result.EstimatedCost = CalculateDeepSeekCost(
                    usage?.PromptTokens ?? 100,
                    usage?.CompletionTokens ?? 700
                );

                return result;
            }
            catch (JsonException)
            {
                // Fallback: Erstelle einfache Struktur
                return new DeepSeekResult
                {
                    Title = "Das Märchen",
                    Story = responseText.Length > 1000
                        ? responseText.Substring(0, 1000) + "..."
                        : responseText,
                    Moral = "Die Moral der Geschichte",
                    Characters = new List<string> { "Held", "Freund" },
                    ImagePrompts = new List<string>
                    {
                        "Magical forest with talking animals, children's book illustration",
                        "Main character discovering a secret path, vibrant colors",
                        "Exciting chase scene through fantasy landscape",
                        "Happy ending with all characters together"
                    },
                    EstimatedCost = CalculateDeepSeekCost(100, 700)
                };
            }
        }

        private decimal CalculateDeepSeekCost(int inputTokens, int outputTokens)
        {
            return (inputTokens * DEEPSEEK_INPUT_PRICE / 1000) +
                   (outputTokens * DEEPSEEK_OUTPUT_PRICE / 1000);
        }

        private decimal CalculateOpenAIImageCost(int imageCount, bool isHd)
        {
            var pricePerImage = isHd ? DALL_E_3_HD_PRICE : DALL_E_3_STANDARD_PRICE;
            return imageCount * pricePerImage;
        }

        // Response-Klassen für APIs
        private class DeepSeekApiResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();

            [JsonPropertyName("usage")]
            public ApiUsage? Usage { get; set; }

            public class Choice
            {
                [JsonPropertyName("message")]
                public Message Message { get; set; } = new();
            }

            public class Message
            {
                [JsonPropertyName("content")]
                public string Content { get; set; } = string.Empty;
            }
        }

        private class ApiUsage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }
        }

        private class OpenAiImageResponse
        {
            [JsonPropertyName("data")]
            public List<ImageData> Data { get; set; } = new();

            public class ImageData
            {
                [JsonPropertyName("url")]
                public string Url { get; set; } = string.Empty;
            }
        }

        private class DeepSeekResult
        {
            public string Title { get; set; } = string.Empty;
            public List<string> Characters { get; set; } = new();
            public string Story { get; set; } = string.Empty;
            public string Moral { get; set; } = string.Empty;
            public List<string> ImagePrompts { get; set; } = new();
            public decimal EstimatedCost { get; set; }
        }
    }
}

