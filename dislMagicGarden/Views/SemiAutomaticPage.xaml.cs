using dislMagicGarden.Models;
using dislMagicGarden.Services;

namespace dislMagicGarden.Views;

public partial class SemiAutomaticPage : FairyBasePage
{
    private readonly IHybridFairyTaleService _fairyTaleService;
    private List<string> _storyHistory = new();
    //private readonly ITextToSpeechService _ttsService;

    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService)
    {
        InitializeComponent();
        _fairyTaleService = fairyTaleService;
       
    }

    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService, string selectedTheme)
    {
        InitializeComponent();
        _fairyTaleService = fairyTaleService;
        ThemeEntry.Text = selectedTheme;
    }

    private async void StartAdventure()
    {
        if (string.IsNullOrEmpty(ThemeEntry.Text))
        {
            await DisplayAlert(Properties.Resources.Error, Properties.Resources.Please_enter_a_topic, "OK");
            return;
        }

        _storyHistory.Clear();
        _storyHistory.Add($"🎯 Thema: {ThemeEntry.Text}");

        SetLoadingState(true);

        try
        {
            var result = await _fairyTaleService.GenerateNextStoryStepAsync(
                ThemeEntry.Text,
                Properties.Resources.Begin_the_adventure,
                _storyHistory
            );
            await UpdateUI(result);
        }
        catch (Exception ex)
        {
            await DisplayAlert(Properties.Resources.Error, ex.Message, "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void OnOptionSelected(object sender, EventArgs e)
    {
        if (sender is not Button button) return;

        string selectedOption = button.Text;

        _storyHistory.Add($"👉 {selectedOption}");

        SetLoadingState(true);

        try
        {
            var result = await _fairyTaleService.GenerateNextStoryStepAsync(
                ThemeEntry.Text,
                selectedOption,
                _storyHistory
            );

            if (result != null)
            {
                await UpdateUI(result);

                if (!string.IsNullOrEmpty(result.Story))
                {
                    _storyHistory.Add($"📖 {result.Story}");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(Properties.Resources.Error, ex.Message, "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task UpdateUI(FairyTaleResponse result)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await AnimateText(result.Story);

            if (result.Options != null && result.Options.Count >= 3)
            {
                Option1Btn.Text = result.Options[0];
                Option2Btn.Text = result.Options[1];
                Option3Btn.Text = result.Options[2];
            }

            await StoryBorder.ScaleTo(1.02, 80);
            await StoryBorder.ScaleTo(1.0, 80);
        });
    }

    private async Task AnimateText(string targetText)
    {
        StoryLabel.Text = "";
        foreach (var c in targetText)
        {
            StoryLabel.Text += c;
            await Task.Delay(20);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;

        Option1Btn.IsEnabled = !isLoading;
        Option2Btn.IsEnabled = !isLoading;
        Option3Btn.IsEnabled = !isLoading;

        StoryBorder.Opacity = isLoading ? 0.5 : 1.0;
    }

    private void OnStartWithThemeClicked(object sender, EventArgs e)
    {
        StartAdventure();
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void ShowHistory_Clicked(object sender, EventArgs e)
    {
        if (_storyHistory.Count == 0)
        {
            await DisplayAlert(Properties.Resources.History, Properties.Resources.No_history_available_yet, "OK");
            return;
        }

        var historyPage = new AdventureHistoryPage();
        historyPage.LoadHistory(_storyHistory, ThemeEntry.Text);

        await Navigation.PushModalAsync(historyPage);
    }
}