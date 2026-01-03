using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace dislMagicGarden.Services
{
   

    public class ImageGeneratorService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        // https://api.together.xyz/settings/api-keys
        private const string ApiKey = "295ca86aee0fa946e5398a216deb109147bc05b63a6eaa6a32312bce0a5ca94d"; // In Produktion sicher speichern!

        public ImageGeneratorService()
        {
        }

        public async Task<string?> GenerateColoringPage(string theme)
        {
            // Der "Coloring Page" System-Prompt für beste Ergebnisse
            string fullPrompt = $"A colorless coloring page for kids, {theme}," +
                $"strictly black and white, no colors, no shading, black outlines only, white background, clean line art on white paper.";

            var requestBody = new
            {
                model = "black-forest-labs/FLUX.1-schnell", // Extrem schnell & günstig
                prompt = fullPrompt,
                width = 1024,
                height = 1024,
                steps = 4, // Reicht für "schnell" Modelle völlig aus
                n = 1,
                response_format = "url"
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var response = await _httpClient.PostAsJsonAsync("https://api.together.xyz/v1/images/generations", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TogetherResponse>();
                return result?.Data?.FirstOrDefault()?.Url;
            }

            return null;
        }

        // Hilfsklassen für die API-Antwort
        private class TogetherResponse { public List<ImageData> Data { get; set; } }
        private class ImageData { public string Url { get; set; } }
    }
}
