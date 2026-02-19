using dislMagicGarden.Models;
using OpenAI.Chat;
using OpenAI.Images;


//using OpenAI.Chat;
//using OpenAI.Images;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace dislMagicGarden.Services
{

    public interface IFairyTaleService
    {
        Task<FairyTaleResponse> GenerateFairyTaleAsync(FairyTaleRequest request);
        Task<decimal> EstimateCostAsync(FairyTaleRequest request);
    }

    public class OpenAIFairyTaleService : IFairyTaleService
    {
        private readonly ChatClient _chatClient;
        private readonly ImageClient _imageClient;
        private readonly IConnectivity _connectivity;

        // Preise pro Unit (Stand: 2024)
        private const decimal GPT4_TURBO_INPUT_PRICE = 0.00001M;   // $0.01 pro 1K Tokens
        private const decimal GPT4_TURBO_OUTPUT_PRICE = 0.00003M;  // $0.03 pro 1K Tokens
        private const decimal DALL_E_3_STANDARD_PRICE = 0.040M;    // $0.04 pro Bild
        private const decimal DALL_E_3_HD_PRICE = 0.080M;          // $0.08 pro Bild (HD)

        public OpenAIFairyTaleService(string apiKey, IConnectivity connectivity)
        {
            _connectivity = connectivity;

            // Initialize OpenAI Clients
            _chatClient = new ChatClient("gpt-4-turbo-preview", apiKey);
            _imageClient = new ImageClient("dall-e-3", apiKey);
        }

        public async Task<FairyTaleResponse> GenerateFairyTaleAsync(FairyTaleRequest request)
        {
            if (_connectivity.NetworkAccess !=  NetworkAccess.Internet)
                throw new NoInternetException("Keine Internetverbindung");

            var stopwatch = Stopwatch.StartNew();
            var response = new FairyTaleResponse();
            var cost = new CostBreakdown();

            try
            {
                // 1. SYSTEM PROMPT für JSON Formatierung
                var systemPrompt = CreateSystemPrompt(request);

                // 2. USER PROMPT mit Thema
                var userPrompt = CreateUserPrompt(request);

                // 3. Text-Generierung (immer)
                var chatResponse = await _chatClient.CompleteChatAsync(
                    messages: [systemPrompt, userPrompt]
                    //temperature: 0.8,
                    //maxTokens: 1500
                );

                // 4. Kosten für Text berechnen
                cost.TextCost = CalculateTextCost(
                    userPrompt.Content.ToString().Length / 4, // Schätzung Input Tokens
                    chatResponse.Value.Usage.OutputTokenCount
                );

                // 5. JSON Response parsen
                var fairyTaleData = ParseFairyTaleResponse(chatResponse.Value .Content[0].Text);
                response.Title = fairyTaleData.Title;
                response.Characters = fairyTaleData.Characters;
                response.Story = fairyTaleData.Story;
                response.Moral = fairyTaleData.Moral;
                //response.ImagePrompts = fairyTaleData.ImagePrompts;

                // 6. Bilder nur bei FullStory Mode
                //if (request.Mode == GenerationMode.FullStory)
                //{
                //    var imageTasks = response.ImagePrompts
                //        .Take(request.ImageCount)
                //        .Select(prompt => GenerateImageAsync(prompt, request.Style))
                //        .ToList();

                //    var images = await Task.WhenAll(imageTasks);
                //    response.ImageUrls = images.ToList();

                //    // Kosten für Bilder berechnen
                //    cost.ImageCost = CalculateImageCost(
                //        request.ImageCount,
                //        isHd: request.Style == "HD"
                //    );
                //}

                // Gesamtkosten
                cost.TotalCost = cost.TextCost + cost.ImageCost;
                response.Cost = cost;
                response.GenerationTime = stopwatch.Elapsed;

                return response;
            }
            catch (Exception ex)
            {
                throw new FairyTaleGenerationException(
                    $"Fehler bei der Generierung: {ex.Message}",
                    ex
                );
            }
        }

        public Task<decimal> EstimateCostAsync(FairyTaleRequest request)
        {
            // Schätzung basierend auf durchschnittlicher Token-Länge
            decimal estimatedCost = 0;

            // Textkosten (durchschnittlich 800 Output-Tokens)
            estimatedCost += 800 * GPT4_TURBO_OUTPUT_PRICE / 1000;

            // Bildkosten nur bei FullStory
            if (request.Mode == GenerationMode.FullStory)
            {
                var imagePrice = request.Style == "HD" ? DALL_E_3_HD_PRICE : DALL_E_3_STANDARD_PRICE;
                estimatedCost += request.ImageCount * imagePrice;
            }

            return Task.FromResult(estimatedCost);
        }

        private SystemChatMessage CreateSystemPrompt(FairyTaleRequest request)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Du bist ein professioneller Märchenerzähler.");
            prompt.AppendLine($"Zielgruppe: Kinder von {request.AgeGroup} Jahren.");
            prompt.AppendLine("Antworte NUR im folgenden JSON Format:");
            prompt.AppendLine("""
                {
                    "title": "Titel des Märchens",
                    "characters": ["Charakter 1", "Charakter 2"],
                    "story": "Die komplette Geschichte hier...",
                    "moral": "Die Moral der Geschichte",
                    "image_prompts": [
                        "Detaillierte Bildbeschreibung für Szene 1",
                        "Detaillierte Bildbeschreibung für Szene 2",
                        "Detaillierte Bildbeschreibung für Szene 3",
                        "Detaillierte Bildbeschreibung für Szene 4"
                    ]
                }
                """);

            return new SystemChatMessage(prompt.ToString());
        }

        private UserChatMessage CreateUserPrompt(FairyTaleRequest request)
        {
            var styleDescription = request.Style switch
            {
                "Classic" => "klassischer Märchenstil (Gebrüder Grimm)",
                "Modern" => "moderner, kindgerechter Stil",
                "Funny" => "lustiger, humorvoller Stil",
                "HD" => "detaillierter, bildhafter Stil für hochwertige Illustrationen",
                _ => "klassischer Märchenstil"
            };

            return new UserChatMessage(
                $"Thema: {request.Theme}\n" +
                $"Stil: {styleDescription}\n" +
                $"Dauer: {request.Duration_min} Minuten\n" +
                $"Bitte generiere ein vollständiges Märchen."
            );
        }

        private async Task<string> GenerateImageAsync(string prompt, string style)
        {
            var imageQuality = style == "HD" ?
                GeneratedImageQuality.High :
                GeneratedImageQuality.Standard;

            var imageStyle = style == "HD" ?
                GeneratedImageStyle.Vivid :
                GeneratedImageStyle.Natural;

            var options = new ImageGenerationOptions
            {
                Quality = imageQuality,
                Style = imageStyle,
                ResponseFormat = GeneratedImageFormat.Uri
            };

            var imageResponse = await _imageClient.GenerateImageAsync(
                prompt: $"Fairy tale illustration, children's book style: {prompt}", options
            );

            return imageResponse.Value.ImageUri.AbsoluteUri;
        }

        private FairyTaleData ParseFairyTaleResponse(string jsonResponse)
        {
            try
            {
                // JSON parsing mit Error Handling
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<FairyTaleData>(jsonResponse, options);

                if (data == null)
                    throw new JsonException("JSON konnte nicht geparst werden");

                return data;
            }
            catch (JsonException)
            {
                // Fallback: Versuche JSON aus Text zu extrahieren
                return ExtractFairyTaleFromText(jsonResponse);
            }
        }

        private FairyTaleData ExtractFairyTaleFromText(string text)
        {
            // Einfache Regex-Extraktion falls JSON-Parsing fehlschlägt
            return new FairyTaleData
            {
                Title = "Generiertes Märchen",
                Story = text,
                Moral = "Sei freundlich zu anderen",
                ImagePrompts = new List<string>
                {
                    "Eine magische Waldlandschaft",
                    "Hauptcharakter trifft auf magisches Wesen",
                    "Spannende Abenteuerszene",
                    "Happy End mit allen Charakteren"
                }
            };
        }

        private decimal CalculateTextCost(int inputTokens, int outputTokens)
        {
            var inputCost = inputTokens * GPT4_TURBO_INPUT_PRICE / 1000;
            var outputCost = outputTokens * GPT4_TURBO_OUTPUT_PRICE / 1000;
            return inputCost + outputCost;
        }

        private decimal CalculateImageCost(int imageCount, bool isHd)
        {
            var pricePerImage = isHd ? DALL_E_3_HD_PRICE : DALL_E_3_STANDARD_PRICE;
            return imageCount * pricePerImage;
        }

        // Private Data Klasse für JSON Deserialization
        private class FairyTaleData
        {
            public string Title { get; set; } = string.Empty;
            public List<string> Characters { get; set; } = new();
            public string Story { get; set; } = string.Empty;
            public string Moral { get; set; } = string.Empty;
            public List<string> ImagePrompts { get; set; } = new();
        }
    }

    // Custom Exceptions
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

