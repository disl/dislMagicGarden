using dislMagicGarden.Models;
using System.Net.Http.Json;

namespace dislMagicGarden.Services;

public class IllustrationService : IIllustrationService
{
    private readonly HttpClient _http;
    private readonly IllustrationSettings _settings;
    private readonly string _imageOutputFolder;

    public IllustrationService()
    {
        _http = new HttpClient();
        _settings = new IllustrationSettings();

        _imageOutputFolder = Path.Combine(FileSystem.AppDataDirectory, "illustrations");
        if (!Directory.Exists(_imageOutputFolder))
            Directory.CreateDirectory(_imageOutputFolder);
    }

    public async Task GenerateIllustrationsAsync(Story story)
    {
        foreach (var chapter in story.Chapters)
        {
            // Prompt erzeugen
            string prompt = BuildPrompt(story, chapter);

            // Bild von KI holen
            var imageBytes = await GenerateImageAsync(prompt);

            // Bild speichern
            string filePath = await SaveImageAsync(chapter.Number, imageBytes);

            // Model updaten
            chapter.ImageOriginalPath = filePath;
        }
    }

    private string BuildPrompt(Story story, Chapter chapter)
    {
        return $@"
                Cute children's book illustration.
                Story: '{story.Title}'
                Chapter summary: '{chapter.Text}'

                Style: {_settings.Style}
                Color palette: {_settings.ColorTheme}
                ";
    }

    /// <summary>
    /// OpenAI / Stable Diffusion API Aufruf
    /// </summary>
    private async Task<byte[]> GenerateImageAsync(string prompt)
    {
        // Beispiel für OpenAI DALL·E-API
        var request = new
        {
            model = "gpt-image-1",
            prompt = prompt,
            size = $"{_settings.Width}x{_settings.Height}"
        };

        var response = await _http.PostAsJsonAsync("https://api.openai.com/v1/images/generations", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<dynamic>();

        // Base64 → Byte[]
        string base64 = json.data[0].b64_json.ToString();
        return Convert.FromBase64String(base64);
    }

    private async Task<string> SaveImageAsync(int chapterNumber, byte[] data)
    {
        string fileName = $"chapter_{chapterNumber}_{DateTime.Now:yyyyMMddHHmmss}.png";
        string filePath = Path.Combine(_imageOutputFolder, fileName);

        await File.WriteAllBytesAsync(filePath, data);

        return filePath;
    }
}

