using dislMagicGarden.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dislMagicGarden.Services;

public class StoryService : IStoryService
{
    private readonly HttpClient _http;
    private readonly string _apiKey = "<YOUR_OPENAI_API_KEY>";
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";
    private readonly ILanguageService _language;
    private bool m_isDeepSeekAllowed;

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

    public async Task<Story?> GenerateStoryAsync(StorySettings s)
    {
        string prompt = BuildPrompt(s);
        CompletionResult response = null;

        //var request = new
        //{
        //    model = "gpt-4o-mini",
        //    messages = new[]
        //    {
        //        new { role = "system", content = "You are a professional children's storyteller AI." },
        //        new { role = "user", content = prompt }
        //    }
        //};

        //var response = await _http.PostAsJsonAsync(Endpoint, request);

        var client = new DeepSeekClient();
        var language = "English";

        switch (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
        {
            case "en": language = "English"; break;
            case "es": language = "Spanish"; break;
            case "fr": language = "French"; break;
            case "de": language = "German"; break;
            case "it": language = "Italian"; break;
            case "uk": language = "Ukrainian"; break;
            case "ru": language = "Russian"; break;
        }

        string json = string.Empty;

        try
        {
            response = await client.GetCompletionAsync(prompt, "application/json");
            if (response == null || string.IsNullOrEmpty(response.Content))
            {
                if (Shell.Current  != null)
                    await Shell.Current.DisplayAlert("Error", "No response from DeepSeek.", "OK");
                return null;
            }

            json= response.Content; //ForForOnDeepSeekClicked(json, response);
        }
        catch (CreditIsInsufficientError)
        {
            m_isDeepSeekAllowed = false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Shell.Current  != null)
                    Shell.Current.DisplayAlert("Error", Properties.Resources.insufficient_credit, "OK");
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Shell.Current  != null)
                    Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            });
        }
        finally
        {
            //Activity_Indicator.IsEnabled = false;
            //Activity_Indicator.IsRunning = false;

            ////UpdateStatusLabel();
        }




        if (response == null || string.IsNullOrEmpty(response.Content))
        {
            throw new Exception($"StoryService API Error: ");
        }

        var json_item =  System.Text.Json.JsonSerializer.Deserialize<JsonElement>(response.Content);

        string result = json_item
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return ParseStoryFromText(s, result);
    }

    private string? ForForOnDeepSeekClicked(string json, CompletionResult response)
    {
        dynamic? jsonObject = null;

        try
        {
            //if (!response.Content.Contains("```json") && !IsJSONString(response.Content))
            //    return null;


            //var json_start_ind = response.Content.IndexOf("```json");
            //var json_end_ind = response.Content.LastIndexOf("```");
            //if (json_start_ind < 0 || json_end_ind < 0 || json_end_ind <= json_start_ind)
            //{
            //    if (Shell.Current  != null)
            //        Shell.Current.DisplayAlert("Error", "Invalid response format from DeepSeek.", "OK");
            //    return null;
            //}
            //json = response.Content.Substring(json_start_ind, json_end_ind - json_start_ind);
            //json = json.Replace("json", "").Replace("```", "").Trim();


        }

        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Shell.Current  != null)
                    Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            });
        }
        return json;
    }

    private bool IsJSONString(string content)
    {
        bool ret_val = false;
        try
        {
            var obj = JsonConvert.DeserializeObject(content);
            ret_val = true;
        }
        catch
        {
            ret_val = false;
        }
        return ret_val;
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
Output language: {s.LanguageIso}
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