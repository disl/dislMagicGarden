using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace dislMagicGarden.Views;

public partial class SketchPage : FairyBasePage
{
    private readonly IConfiguration _configuration;
    string apiUrl = "https://api.together.xyz/v1/chat/completions";
    private const string together_key = "295ca86aee0fa946e5398a216deb109147bc05b63a6eaa6a32312bce0a5ca94d"; // In Produktion sicher speichern!

    public SketchPage(IConfiguration configuration)
    {
        InitializeComponent();

        _configuration = configuration;
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        MainDrawingView.Clear();
        DeepSeekResult.Text = Properties.Resources.A_blank_page_for_a_new_adventure;
    }

    private async void OnRecognizeSketchClicked(object sender, EventArgs e)
    {
        if (MainDrawingView.Lines.Count == 0)
        {
            await DisplayAlert(Properties.Resources.Error, Properties.Resources.Oops_Draw_something_first_before_we_do_magic, "OK");
            return;
        }

        LoadingSpinner.IsRunning = true;
        DeepSeekResult.Text = Properties.Resources.Im_thinking;

        try
        {
            // 1. Skizze als Stream exportieren (Format & Größe)
            using var imageStream = await MainDrawingView.GetImageStream(800, 800);

            // 2. In Byte-Array umwandeln
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();

            // 3. Als Base64 kodieren
            string base64Image = Convert.ToBase64String(imageBytes);

            // 4. An DeepSeek senden (mit angepasstem Prompt)
            await SendToTogetherAI(base64Image);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
        }
    }

    private async Task SendToTogetherAI(string base64Image)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(apiUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", together_key);

        var payload = new
        {
            // Llama 3.2 Vision ist bei Together extrem günstig und schnell
            model = "meta-llama/Llama-4-Maverick-17B-128E-Instruct-FP8",
            messages = new[]
        {
            new
            {
                role = "user",
                content = new object[]
                {
                    new {
                        type = "text",
                        text = "Du bist ein magischer Märchenerzähler. " +
                        "Was siehst du auf diesem Bild? Beschreibe es kurz und fantasievoll für ein Kind."
                    },
                    new {
                        type = "image_url",
                        image_url = new { url = $"data:image/jpeg;base64,{base64Image}" }
                    }
                }
            }
        },
            max_tokens = 512,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Schnelles Extrahieren des Contents ohne große Klassen-Struktur
                var node = JsonNode.Parse(jsonResponse);
                var result_content = node?["choices"]?[0]?["message"]?["content"]?.ToString();

                DeepSeekResult.Text = result_content;
            }
            else
            {
                DeepSeekResult.Text = Properties.Resources.I_cannot_recognise_the_object;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}