using System.Net.Http.Json;
using dislMagicGarden.Models;
using System.Text.Json;

namespace dislMagicGarden.Services;

public class StoryService : IStoryService
{
    private readonly HttpClient _http;
    private readonly string _apiKey = "<YOUR_OPENAI_API_KEY>";
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";
    private readonly ILanguageService _language;

    public StoryService(ILanguageService languageService)
    {
        _language = languageService;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<Story> GenerateStoryAsync(string childName, string sidekickAnimal, string worldSetting, string mood)
    {
        var settings = new StorySettings
        {
            ChildName = childName,
            SidekickAnimal = sidekickAnimal,
            WorldSetting = worldSetting,
            Mood = mood,
            ChapterCount = 4
        };

        return await GenerateStoryAsync(settings);
    }

    public async Task<Story> GenerateStoryAsync(StorySettings s)
    {
        string prompt = BuildPrompt(s);

        var request = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are a professional children's storyteller AI." },
                new { role = "user", content = prompt }
            }
        };

        var response = await _http.PostAsJsonAsync(Endpoint, request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"StoryService API Error: {response.StatusCode}");
        }

        var json = await JsonSerializer.DeserializeAsync<JsonElement>(
            await response.Content.ReadAsStreamAsync()
        );

        string result = json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return ParseStoryFromText(s, result);
    }

    // --------------------------------------------------------------
    //  PROMPT BUILDER (English → Output Language = German)
    // --------------------------------------------------------------
    private string BuildPrompt(StorySettings s)
    {
        string Language = _language.Resolve(s.LanguageIso);

        return $@"
You are a professional children's storyteller AI.
Please write the ENTIRE OUTPUT in: {Language}.

Write a magical, soft bedtime story for a little girl.
Use a warm, pastel, emotional tone.
Write visually, with enough detail to inspire illustrations.
Each chapter should have 4–6 sentences.

OUTPUT FORMAT (STRICT):
### Title
<the story title>

### Chapter 1
<chapter content>

### Chapter 2
<chapter content>

...
### Chapter {s.ChapterCount}
<chapter content>

VARIABLES:
Child's name: {s.ChildName}
Sidekick animal: {s.SidekickAnimal}
Fantasy world: {s.WorldSetting}
Mood: {s.Mood}
Chapters: {s.ChapterCount}
";
    }

    // --------------------------------------------------------------
    //  PARSER — Wandelt GPT-Ausgabe in Story + Kapitel um
    // --------------------------------------------------------------
    private Story ParseStoryFromText(StorySettings settings, string text)
    {
        var story = new Story
        {
            Title = "",
            ChildName = settings.ChildName,
            Chapters = new List<Chapter>()
        };

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        string currentChapterText = "";
        int chapterNumber = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("### Title", StringComparison.OrdinalIgnoreCase))
                continue;

            if (chapterNumber == 0 && !line.StartsWith("### Chapter"))
            {
                story.Title = line;
                continue;
            }

            if (line.StartsWith("### Chapter"))
            {
                if (chapterNumber > 0)
                {
                    story.Chapters.Add(new Chapter
                    {
                        Number = chapterNumber,
                        Text = currentChapterText.Trim()
                    });
                }

                chapterNumber++;
                currentChapterText = "";
                continue;
            }

            currentChapterText += line + "\n";
        }

        if (!string.IsNullOrWhiteSpace(currentChapterText))
        {
            story.Chapters.Add(new Chapter
            {
                Number = chapterNumber,
                Text = currentChapterText.Trim()
            });
        }

        return story;
    }
}