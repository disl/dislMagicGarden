using dislMagicGarden.Models;
using dislMagicGarden.Services;
using System.Collections.ObjectModel;

namespace dislMagicGarden.Views;

public partial class SemiAutomaticPage : FairyBasePage
{
    private readonly IHybridFairyTaleService _fairyTaleService;
    private List<string> _storyHistory = new();
    public ObservableCollection<string> StoryHistoryDisplay { get; } = new();

    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService)
    {
        InitializeComponent();
        _fairyTaleService = fairyTaleService;
        BindingContext = this;
    }

    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService, string selectedTheme)
    {
        InitializeComponent();
        _fairyTaleService = fairyTaleService;
        ThemeEntry.Text = selectedTheme;
        BindingContext = this;
    }

    private async void StartAdventure()
    {
        if (string.IsNullOrEmpty(ThemeEntry.Text))
        {
            await DisplayAlert("Fehler", "Bitte gib ein Thema ein!", "OK");
            return;
        }

        // History zurücksetzen und neuen Eintrag hinzufügen
        _storyHistory.Clear();
        StoryHistoryDisplay.Clear();

        _storyHistory.Add(ThemeEntry.Text);
        StoryHistoryDisplay.Add($"🎯 Start: {ThemeEntry.Text}");

        SetLoadingState(true);

        try
        {
            var result = await _fairyTaleService.GenerateNextStoryStepAsync(
                ThemeEntry.Text,
                "Beginne das Abenteuer",
                _storyHistory
            );
            await UpdateUI(result);
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

        // Zur History hinzufügen
        _storyHistory.Add(selectedOption);
        StoryHistoryDisplay.Add($"👉 {selectedOption}");

        // Automatisch nach unten scrollen
        if (StoryHistoryDisplay.Count > 0)
        {
            await Task.Delay(100);
            HistoryCollectionView.ScrollTo(StoryHistoryDisplay.Count - 1, position: ScrollToPosition.End);
        }

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

                // Story-Text auch zur History hinzufügen (optional)
                if (!string.IsNullOrEmpty(result.Story))
                {
                    var shortStory = result.Story.Length > 50
                        ? result.Story.Substring(0, 50) + "..."
                        : result.Story;
                    StoryHistoryDisplay.Add($"📖 {shortStory}");

                    // Wieder nach unten scrollen
                    await Task.Delay(100);
                    HistoryCollectionView.ScrollTo(StoryHistoryDisplay.Count - 1, position: ScrollToPosition.End);
                }
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
            await Task.Delay(25);
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

    // History-Methoden
    private void ShowHistory_Clicked(object sender, EventArgs e)
    {
        HistoryPopup.IsVisible = !HistoryPopup.IsVisible;

        if (HistoryPopup.IsVisible && StoryHistoryDisplay.Count > 0)
        {
            // Nach unten scrollen, um die neuesten Einträge zu sehen
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                HistoryCollectionView.ScrollTo(StoryHistoryDisplay.Count - 1, position: ScrollToPosition.End);
            });
        }
    }

    private void HideHistory_Clicked(object sender, EventArgs e)
    {
        HistoryPopup.IsVisible = false;
    }

    private async void ClearHistory_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert(
            "Verlauf löschen",
            "Möchtest du wirklich den gesamten Verlauf löschen? Das Abenteuer geht weiter, aber der Verlauf wird geleert.",
            "Ja, löschen",
            "Abbrechen"
        );

        if (answer)
        {
            _storyHistory.Clear();
            StoryHistoryDisplay.Clear();

            // Nur das Start-Thema wieder hinzufügen, falls vorhanden
            if (!string.IsNullOrEmpty(ThemeEntry.Text))
            {
                StoryHistoryDisplay.Add($"🎯 Start: {ThemeEntry.Text}");
            }
        }
    }
}


//using dislMagicGarden.Models;
//using dislMagicGarden.Services;

//namespace dislMagicGarden.Views;

//public partial class SemiAutomaticPage : FairyBasePage
//{
//    private readonly IHybridFairyTaleService _fairyTaleService;
//    private List<string> _storyHistory = new();
//    //private string _currentTheme;  // = Properties.Resources.A_brave_squirrel_saves_the_forest;

//    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService)
//    {
//        InitializeComponent();

//        _fairyTaleService = fairyTaleService;
//        //_currentTheme = selectedTheme;

//        // Startet das Abenteuer automatisch beim Öffnen
//        // StartAdventure();
//    }



//    // Konstruktor mit Dependency Injection (oder über ServiceHelper)
//    public SemiAutomaticPage(IHybridFairyTaleService fairyTaleService, string selectedTheme)
//    {
//        InitializeComponent();

//        _fairyTaleService = fairyTaleService;
//        ThemeEntry.Text = selectedTheme;

//        // Startet das Abenteuer automatisch beim Öffnen
//        //StartAdventure();
//    }

//    private async void StartAdventure()
//    {
//        if (string.IsNullOrEmpty(ThemeEntry.Text))
//        {
//            await Application.Current.MainPage.DisplayAlert(Properties.Resources.Error, Properties.Resources.Please_enter_a_topic, "OK");
//            return;
//        }

//        _storyHistory.Add(ThemeEntry.Text); 

//        SetLoadingState(true);

//        try
//        {


//            // Initialer Aufruf ohne vorherige Wahl
//            var result = await _fairyTaleService.GenerateNextStoryStepAsync(ThemeEntry.Text, "Beginne das Abenteuer", _storyHistory);
//            await UpdateUI(result);
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Startfehler", ex.Message, "OK");
//        }
//        finally
//        {
//            SetLoadingState(false);
//        }
//    }

//    private async void OnOptionSelected(object sender, EventArgs e)
//    {
//        if (sender is not Button button) return;

//        string selectedOption = button.Text;
//        SetLoadingState(true);

//        try
//        {
//            _storyHistory.Add(selectedOption);

//            // Begrenzung der Historie (optional, damit der Context nicht zu groß wird)
//            //if (_storyHistory.Count > 10) _storyHistory.RemoveAt(0);

//            var result = await _fairyTaleService.GenerateNextStoryStepAsync(
//                ThemeEntry.Text,
//                selectedOption,
//                _storyHistory
//            );

//            if (result != null)
//            {
//                await UpdateUI(result);
//            }
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Oh weh!", "Der Zauberstab klemmt gerade: " + ex.Message, "Nochmal versuchen");
//        }
//        finally
//        {
//            SetLoadingState(false);
//        }
//    }

//    private async Task UpdateUI(FairyTaleResponse result)
//    {
//        MainThread.BeginInvokeOnMainThread(async () =>
//        {
//            // Titel setzen
//            //TitleLabel.Text = result.Title;

//            // Text mit Typewriter-Effekt einblenden
//            await AnimateText(result.Story);

//            // Buttons aktualisieren (Sicherheitshalber prüfen, ob Optionen da sind)
//            if (result.Options != null && result.Options.Count >= 3)
//            {
//                Option1Btn.Text = result.Options[0];
//                Option2Btn.Text = result.Options[1];
//                Option3Btn.Text = result.Options[2];
//            }

//            // Kleiner visueller Effekt für den Border
//            await StoryBorder.ScaleTo(1.03, 100);
//            await StoryBorder.ScaleTo(1.0, 100);
//        });
//    }

//    private async Task AnimateText(string targetText)
//    {
//        StoryLabel.Text = "";
//        foreach (var c in targetText)
//        {
//            StoryLabel.Text += c;
//            await Task.Delay(25); // Geschwindigkeit des Schreibens
//        }
//    }

//    private void SetLoadingState(bool isLoading)
//    {
//        LoadingIndicator.IsRunning = isLoading;
//        LoadingIndicator.IsVisible = isLoading;

//        // Buttons während des Ladens deaktivieren
//        Option1Btn.IsEnabled = !isLoading;
//        Option2Btn.IsEnabled = !isLoading;
//        Option3Btn.IsEnabled = !isLoading;

//        // Karte leicht ausgrauen
//        StoryBorder.Opacity = isLoading ? 0.5 : 1.0;
//    }

//    private void OnStartWithThemeClicked(object sender, EventArgs e)
//    {

//        StartAdventure();
//    }

//    private async void Close_Clicked(object sender, EventArgs e)
//    {
//        await Shell.Current.GoToAsync("//HomePage");
//    }
//}