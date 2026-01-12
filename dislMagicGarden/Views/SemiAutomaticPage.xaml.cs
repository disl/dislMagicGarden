using dislMagicGarden.Models;
using dislMagicGarden.Services;

namespace dislMagicGarden.Views;

public partial class SemiAutomaticPage : FairyBasePage
{
	public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService)
	{
		InitializeComponent();

        _fairyTaleService = fairyTaleService;
        //_currentTheme = selectedTheme;

        // Startet das Abenteuer automatisch beim Öffnen
        StartAdventure();
    }

    private readonly IHybridFairyTaleService _fairyTaleService;
    private List<string> _storyHistory = new();
    private string _currentTheme;

    // Konstruktor mit Dependency Injection (oder über ServiceHelper)
    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService, string selectedTheme )
    {
        InitializeComponent();

        _fairyTaleService = fairyTaleService;
        _currentTheme = selectedTheme;

        // Startet das Abenteuer automatisch beim Öffnen
        StartAdventure();
    }

    private async void StartAdventure()
    {
        SetLoadingState(true);
        try
        {
            // Initialer Aufruf ohne vorherige Wahl
            var result = await _fairyTaleService.GenerateNextStoryStepAsync(_currentTheme, "Beginne das Abenteuer", _storyHistory);
            UpdateUI(result);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Startfehler", ex.Message, "OK");
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
        SetLoadingState(true);

        try
        {
            _storyHistory.Add(selectedOption);

            // Begrenzung der Historie (optional, damit der Context nicht zu groß wird)
            if (_storyHistory.Count > 10) _storyHistory.RemoveAt(0);

            var result = await _fairyTaleService.GenerateNextStoryStepAsync(
                _currentTheme,
                selectedOption,
                _storyHistory
            );

            if (result != null)
            {
                UpdateUI(result);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Oh weh!", "Der Zauberstab klemmt gerade: " + ex.Message, "Nochmal versuchen");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void UpdateUI(FairyTaleResponse result)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Titel setzen
            TitleLabel.Text = result.Title;

            // Text mit Typewriter-Effekt einblenden
            await AnimateText(result.Story);

            // Buttons aktualisieren (Sicherheitshalber prüfen, ob Optionen da sind)
            if (result.Options != null && result.Options.Count >= 3)
            {
                Option1Btn.Text = result.Options[0];
                Option2Btn.Text = result.Options[1];
                Option3Btn.Text = result.Options[2];
            }

            // Kleiner visueller Effekt für den Border
            await StoryBorder.ScaleTo(1.03, 100);
            await StoryBorder.ScaleTo(1.0, 100);
        });
    }

    private async Task AnimateText(string targetText)
    {
        StoryLabel.Text = "";
        foreach (var c in targetText)
        {
            StoryLabel.Text += c;
            await Task.Delay(25); // Geschwindigkeit des Schreibens
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;

        // Buttons während des Ladens deaktivieren
        Option1Btn.IsEnabled = !isLoading;
        Option2Btn.IsEnabled = !isLoading;
        Option3Btn.IsEnabled = !isLoading;

        // Karte leicht ausgrauen
        StoryBorder.Opacity = isLoading ? 0.5 : 1.0;
    }
}