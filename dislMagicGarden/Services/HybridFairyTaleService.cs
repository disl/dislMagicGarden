using dislMagicGarden.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI; // Version 2.7.0
using OpenAI.Images;
using System.Diagnostics;
// Services/HybridFairyTaleService.cs
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dislMagicGarden.Services
{


    namespace YourMauiApp.Services
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
            private readonly OpenAIClient _openAIClient;
            private readonly IConnectivity _connectivity;
            private readonly ILogger<HybridFairyTaleService> _logger;

            // API Keys
            private readonly string _deepSeekApiKey;
            private readonly string _openAiApiKey;

            // DeepSeek Endpoints
            private const string DEEPSEEK_API_URL = "https://api.deepseek.com/v1/chat/completions";
            private const string DEEPSEEK_MODEL = "deepseek-chat";

            // Preise
            private const decimal DALL_E_3_STANDARD_PRICE = 0.040M;
            private const decimal DALL_E_3_HD_PRICE = 0.080M;
            private const decimal DEEPSEEK_INPUT_PRICE = 0.0000014M;
            private const decimal DEEPSEEK_OUTPUT_PRICE = 0.0000028M;

            public HybridFairyTaleService(
                IConfiguration config,
                IConnectivity connectivity,
                ILogger<HybridFairyTaleService> logger)
            {
                _connectivity = connectivity;
                _logger = logger;
                _httpClient = new HttpClient();

                // API Keys holen
                _deepSeekApiKey = config["DeepSeek:ApiKey"]
                    ?? throw new ArgumentException("DeepSeek API Key fehlt");
                _openAiApiKey = config["OpenAI:ApiKey"]
                    ?? "optional";

                // OpenAI Client für Bilder
                if (!string.IsNullOrEmpty(_openAiApiKey))
                {
                    _openAIClient = new OpenAIClient(_openAiApiKey);
                }

                // DeepSeek Auth Header
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_deepSeekApiKey}");
            }

            // KORRIGIERT für OpenAI 2.7.0
            public async Task<FairyTaleResponse> GenerateFairyTaleAsync(FairyTaleRequest request)
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                    throw new NoInternetException("Keine Internetverbindung");

                var stopwatch = Stopwatch.StartNew();
                var response = new FairyTaleResponse();
                var cost = new CostBreakdown();

                try
                {
                    // 1. TEXT mit DeepSeek
                    var deepSeekResult = await GenerateTextWithDeepSeekAsync(request);

                    // Kosten berechnen
                    cost.TextCost = CalculateDeepSeekCost(
                        deepSeekResult.EstimatedInputTokens,
                        deepSeekResult.EstimatedOutputTokens
                    );

                    response.Title = deepSeekResult.Title;
                    response.Characters = deepSeekResult.Characters;
                    response.Story = deepSeekResult.Story;
                    response.Moral = deepSeekResult.Moral;
                    response.ImagePrompts = deepSeekResult.ImagePrompts;

                    // 2. BILDER mit OpenAI
                    if (request.Mode == GenerationMode.FullStory && _openAIClient != null)
                    {
                        var images = await GenerateImagesWithOpenAIAsync(
                            response.ImagePrompts,
                            request
                        );

                        response.ImageUrls = images;
                        cost.ImageCost = CalculateOpenAIImageCost(
                            request.ImageCount,
                            request.Style == "HD"
                        );
                    }

                    cost.TotalCost = cost.TextCost + cost.ImageCost;
                    response.Cost = cost;
                    response.GenerationTime = stopwatch.Elapsed;

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler bei Märchen-Generierung");
                    throw new FairyTaleGenerationException(
                        $"Fehler bei der Generierung: {ex.Message}",
                        ex
                    );
                }
            }

            // DeepSeek Text Generation (bleibt gleich)
            private async Task<DeepSeekFairyTaleData> GenerateTextWithDeepSeekAsync(FairyTaleRequest request)
            {
                var systemPrompt = CreateDeepSeekSystemPrompt(request);
                var userPrompt = CreateDeepSeekUserPrompt(request);

                var requestBody = new
                {
                    model = DEEPSEEK_MODEL,
                    messages = new[]
                    {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                    temperature = 0.8,
                    max_tokens = 1500,
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(DEEPSEEK_API_URL, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var deepSeekResponse = JsonSerializer.Deserialize<DeepSeekApiResponse>(responseJson);

                if (deepSeekResponse?.Choices?.FirstOrDefault()?.Message?.Content == null)
                    throw new Exception("DeepSeek API returned invalid response");

                var contentText = deepSeekResponse.Choices[0].Message.Content;

                int inputTokens = deepSeekResponse.Usage?.PromptTokens ?? 150;
                int outputTokens = deepSeekResponse.Usage?.CompletionTokens ?? 800;

                return ParseDeepSeekResponse(contentText, inputTokens, outputTokens);
            }

            // KORRIGIERT: OpenAI 2.7.0 Image Generation
            private async Task<List<string>> GenerateImagesWithOpenAIAsync(
                List<string> prompts,
                FairyTaleRequest request)
            {
                var images = new List<string>();

                foreach (var prompt in prompts.Take(request.ImageCount))
                {
                    var imageUrl = await GenerateSingleImageAsync(prompt, request.Style);
                    if (!string.IsNullOrEmpty(imageUrl))
                        images.Add(imageUrl);
                }

                return images;
            }

            private async Task<string> GenerateSingleImageAsync(string prompt, string style)
            {
                if (_openAIClient == null)
                    throw new InvalidOperationException("OpenAI API Key nicht konfiguriert");

                try
                {
                    // Für OpenAI 2.7.0 - NEUE API
                    var imageGenerationOptions = new ImageGenerationOptions
                    { 
                        Prompt = $"Children's book illustration, fairy tale style: {prompt}",
                        Model = "dall-e-3", // Explizit angeben
                        Quality = style == "HD" ? ImageGenerationQuality.Hd : ImageGenerationQuality.Standard,
                        Style = style == "HD" ? ImageGenerationStyle.Vivid : ImageGenerationStyle.Natural,
                        Size = ImageGenerationSize.W1792xH1024, // Breitformat
                        ResponseFormat = ImageGenerationResponseFormat.Url
                    };

                    var response = await _openAIClient.GetImageClient()
                        .GenerateImageAsync(imageGenerationOptions);

                    // URL aus Response extrahieren
                    if (response?.Value?.Data?.FirstOrDefault()?.Url != null)
                    {
                        return response.Value.Data.First().Url.AbsoluteUri;
                    }

                    throw new Exception("Keine Bild-URL in der Response erhalten");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler bei Bildgenerierung");
                    throw;
                }
            }

            // Text-Only Methode
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
                    EstimatedCost = CalculateDeepSeekCost(
                        result.EstimatedInputTokens,
                        result.EstimatedOutputTokens
                    )
                };
            }

            public Task<decimal> EstimateCostAsync(FairyTaleRequest request)
            {
                decimal estimatedCost = 0;

                // DeepSeek Textkosten
                estimatedCost += 800 * DEEPSEEK_OUTPUT_PRICE / 1000;

                // OpenAI Bildkosten
                if (request.Mode == GenerationMode.FullStory)
                {
                    var imagePrice = request.Style == "HD"
                        ? DALL_E_3_HD_PRICE
                        : DALL_E_3_STANDARD_PRICE;
                    estimatedCost += request.ImageCount * imagePrice;
                }

                return Task.FromResult(estimatedCost);
            }

            // Helper Methods (bleiben gleich)
            private string CreateDeepSeekSystemPrompt(FairyTaleRequest request)
            {
                return $$"""
                Du bist ein professioneller Märchenerzähler für Kinder von {{request.AgeGroup}} Jahren.
                Stil: {{GetStyleDescription(request.Style)}}.
                
                Antworte IMMER in diesem JSON-Format:
                {
                    "title": "Titel",
                    "characters": ["Char1", "Char2"],
                    "story": "Geschichte hier...",
                    "moral": "Moral",
                    "image_prompts": [
                        "English image description for scene 1",
                        "English image description for scene 2",
                        "English image description for scene 3",
                        "English image description for scene 4"
                    ]
                }
                """;
            }

            private string CreateDeepSeekUserPrompt(FairyTaleRequest request)
            {
                return $"Thema: {request.Theme}\nErstelle ein komplettes Märchen.";
            }

            private DeepSeekFairyTaleData ParseDeepSeekResponse(string content, int inputTokens, int outputTokens)
            {
                try
                {
                    // JSON extrahieren
                    var jsonStart = content.IndexOf('{');
                    var jsonEnd = content.LastIndexOf('}') + 1;

                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonText = content.Substring(jsonStart, jsonEnd - jsonStart);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var data = JsonSerializer.Deserialize<DeepSeekFairyTaleData>(jsonText, options);

                        if (data != null)
                        {
                            data.EstimatedInputTokens = inputTokens;
                            data.EstimatedOutputTokens = outputTokens;
                            return data;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fehler beim JSON Parsing");
                }

                // Fallback
                return new DeepSeekFairyTaleData
                {
                    Title = "Das Märchen",
                    Story = content,
                    Moral = "Die Moral der Geschichte",
                    Characters = new List<string> { "Held", "Freund" },
                    ImagePrompts = new List<string>
                {
                    "Magical forest with glowing mushrooms, children's book illustration",
                    "Main character meeting magical creature, vibrant colors",
                    "Adventure scene with obstacles, dynamic composition",
                    "Happy ending celebration, warm lighting"
                },
                    EstimatedInputTokens = inputTokens,
                    EstimatedOutputTokens = outputTokens
                };
            }

            private decimal CalculateDeepSeekCost(int inputTokens, int outputTokens)
            {
                var inputCost = inputTokens * DEEPSEEK_INPUT_PRICE / 1000;
                var outputCost = outputTokens * DEEPSEEK_OUTPUT_PRICE / 1000;
                return inputCost + outputCost;
            }

            private decimal CalculateOpenAIImageCost(int imageCount, bool isHd)
            {
                var pricePerImage = isHd ? DALL_E_3_HD_PRICE : DALL_E_3_STANDARD_PRICE;
                return imageCount * pricePerImage;
            }

            private string GetStyleDescription(string style)
            {
                return style switch
                {
                    "Classic" => "klassischer Märchenstil",
                    "Modern" => "moderner Stil",
                    "Funny" => "lustiger Stil",
                    _ => "klassischer Stil"
                };
            }

            // Response Klassen für DeepSeek API
            private class DeepSeekApiResponse
            {
                [JsonPropertyName("choices")]
                public List<DeepSeekChoice> Choices { get; set; } = new();

                [JsonPropertyName("usage")]
                public DeepSeekUsage? Usage { get; set; }
            }

            private class DeepSeekChoice
            {
                [JsonPropertyName("message")]
                public DeepSeekMessage Message { get; set; } = new();
            }

            private class DeepSeekMessage
            {
                [JsonPropertyName("content")]
                public string Content { get; set; } = string.Empty;
            }

            private class DeepSeekUsage
            {
                [JsonPropertyName("prompt_tokens")]
                public int PromptTokens { get; set; }

                [JsonPropertyName("completion_tokens")]
                public int CompletionTokens { get; set; }
            }

            private class DeepSeekFairyTaleData
            {
                public string Title { get; set; } = string.Empty;
                public List<string> Characters { get; set; } = new();
                public string Story { get; set; } = string.Empty;
                public string Moral { get; set; } = string.Empty;
                public List<string> ImagePrompts { get; set; } = new();
                public int EstimatedInputTokens { get; set; }
                public int EstimatedOutputTokens { get; set; }
            }
        }

        public class TextOnlyResponse
        {
            public string Title { get; set; } = string.Empty;
            public string Story { get; set; } = string.Empty;
            public string Moral { get; set; } = string.Empty;
            public decimal EstimatedCost { get; set; }
        }

        public class NoInternetException : Exception
        {
            public NoInternetException(string message) : base(message) { }
        }

        public class FairyTaleGenerationException : Exception
        {
            public FairyTaleGenerationException(string message, Exception inner)
                : base(message, inner) { }
        }
    }
}
