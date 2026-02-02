using dislMagicGarden.Services;
using System.Collections.ObjectModel;

namespace dislMagicGarden.Views;

public partial class AdventureHistoryPage : FairyBasePage
{
    public ObservableCollection<HistoryItem> HistoryItems { get; } = new();
    private readonly ITextToSpeechService _ttsService;  // = DependencyService.Get<ITextToSpeechService>();
    string Title;

    public AdventureHistoryPage(ITextToSpeechService textToSpeechService)
    {
        InitializeComponent();
        BindingContext = this;
        HistoryCollectionView.ItemsSource = HistoryItems;
        _ttsService = textToSpeechService;
    }

    public void LoadHistory(List<string> history, string title)
    {
        HistoryItems.Clear();

        Title = title;

        for (int i = 0; i < history.Count; i++)
        {
            var item = history[i];
            string icon = "";  // GetIconForStep(i, item);
            string stepNumber = $"{Properties.Resources.Step} {i + 1}";

            HistoryItems.Add(new HistoryItem
            {
                Icon = icon,
                StepNumber = stepNumber,
                Text = item
            });
        }

        // Zum letzten Eintrag scrollen
        if (HistoryItems.Count > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                HistoryCollectionView.ScrollTo(HistoryItems.Count - 1,
                    position: ScrollToPosition.End,
                    animate: false);
            });
        }
    }

    //private string GetIconForStep(int index, string text)
    //{
    //    if (index == 0) return "🎯";
    //    if (text.StartsWith("👉")) return "👉";
    //    if (text.StartsWith("📖")) return "📖";
    //    if (text.Contains("gewählt") || text.Contains("Option")) return "✅";
    //    return "📝";
    //}

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void SpeakStory_Clicked(object sender, EventArgs e)
    {
        var SpeechSpeed = Preferences.Get("speechSpeed", 1f);

        var full_text = string.Join(" ", HistoryItems.Select(x => x.Text.Trim()));
        var normolazed_text = PdfExportService.NormalizeText(full_text);

        await _ttsService.SpeakAsync(normolazed_text);
    }

    private void StopStory_Clicked(object sender, EventArgs e)
    {
        _ttsService.Stop();
    }

    private async void Share_Clicked(object sender, EventArgs e)
    {
        var full_text = string.Join(" ", HistoryItems.Select(x => x.Text.Trim()));

        await Share.RequestAsync(new ShareTextRequest
        {
            Title = Properties.Resources.Share_fairy_tales,
            Text = full_text
        });
    }

    private async void ShowPicture_Clicked(object sender, EventArgs e)
    {
        var full_text = string.Join(" ", HistoryItems.Select(x => x.Text.Trim()));

        await Application.Current.MainPage.Navigation
                      .PushModalAsync(new ColoringGenerator(full_text, Title), true);
    }
}

public class HistoryItem
{
    public string Icon { get; set; }
    public string StepNumber { get; set; }
    public string Text { get; set; }
}