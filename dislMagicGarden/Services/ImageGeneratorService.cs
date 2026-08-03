using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace dislMagicGarden.Services
{
    public class ImageGeneratorService
    {
        private readonly AiSettingsService _settings;

        public ImageGeneratorService(AiSettingsService settings)
        {
            _settings = settings;
        }

        public async Task<string?> GenerateColoringPage(string theme)
        {
            var settings = await _settings.LoadImageAsync();
            if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.BaseUrl))
                throw new AiNotConfiguredException(AiSettingsService.T("SettingsMissingImage"));

            // Der "Coloring Page" System-Prompt für beste Ergebnisse
            string fullPrompt = $"A colorless coloring page for kids, {theme}," +
                $"strictly black and white, no colors, no shading, black outlines only, white background, clean line art on white paper.";

            var requestBody = BuildRequestBody(settings, fullPrompt);

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

            var response = await client.PostAsJsonAsync(BuildEndpoint(settings.BaseUrl), requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TogetherResponse>();
                return result?.Data?.FirstOrDefault()?.Url;
            }

            return null;
        }

        /// <summary>
        /// Validates the image endpoint + key via the cheap GET models endpoint.
        /// </summary>
        public async Task TestImageConnectionAsync(AiImageSettings settings)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

            var response = await client.GetAsync($"{settings.BaseUrl.Trim().TrimEnd('/')}/models");

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"API Fehler {(int)response.StatusCode}: {err[..Math.Min(300, err.Length)]}");
            }
        }

        /// <summary>
        /// The request body differs between the two image providers (Together/FLUX vs OpenAI).
        /// </summary>
        private static object BuildRequestBody(AiImageSettings settings, string fullPrompt)
        {
            if (settings.Provider == AiSettingsService.ImageProviderOpenAi)
            {
                return new
                {
                    model = settings.Model,
                    prompt = fullPrompt,
                    n = 1,
                    size = "1024x1024",
                    response_format = "url"
                };
            }

            return new
            {
                model = settings.Model,
                prompt = fullPrompt,
                width = 1024,
                height = 1024,
                steps = 4, // Reicht für "schnell" Modelle völlig aus
                n = 1,
                response_format = "url"
            };
        }

        /// <summary>
        /// Builds the OpenAI-compatible images URL from a base URL.
        /// </summary>
        private static string BuildEndpoint(string baseUrl)
        {
            var url = baseUrl.Trim().TrimEnd('/');
            if (url.EndsWith("/images/generations", StringComparison.OrdinalIgnoreCase))
                return url;
            return url + "/images/generations";
        }

        // Hilfsklassen für die API-Antwort
        private class TogetherResponse { public List<ImageData> Data { get; set; } }
        private class ImageData { public string Url { get; set; } }
    }
}
