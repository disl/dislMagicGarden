using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using dislMagicGarden.Properties;
using dislMagicGarden.Services;

namespace dislMagicGarden.ViewModels
{

    public partial class FairyTaleViewModel : BaseViewModel
    {
        private readonly IHybridFairyTaleService _fairyTaleService;

        [ObservableProperty]
        private string _theme = "Ein kleiner Drache lernt fliegen";

        [ObservableProperty]
        private string _selectedStyle = "Classic";

        [ObservableProperty]
        private GenerationMode _selectedMode = GenerationMode.TextOnly;

        [ObservableProperty]
        private bool _isGenerating = false;

        [ObservableProperty]
        private FairyTaleResponse? _currentFairyTale;

        [ObservableProperty]
        private string _statusMessage = "Bereit";

        [ObservableProperty]
        private bool _hasError = false;

        [ObservableProperty]
        private string _errorMessage = "";

        public List<string> Styles { get; } = new()
        {
            "Classic", "Modern", "Funny", "HD"
        };

        public List<string> Moods { get; }

        [ObservableProperty]
        string languageIso = "en";

        [ObservableProperty]
        string childName;

        [ObservableProperty]
        string sidekickAnimal;

        [ObservableProperty]
        string worldSetting;

        [ObservableProperty]
        string mood = Properties.Resources.Reassuring;

        private readonly ILanguageService _language;


        public FairyTaleViewModel(IHybridFairyTaleService fairyTaleService, ILanguageService language)
        {
            _fairyTaleService = fairyTaleService;
            Title = Properties.Resources.Home_NewStory;
            _language=language;

            Moods = new()
            {
                Resources.Reassuring,
                Resources.Adventurous,
                Resources.Funny
            };
        }

        [RelayCommand]
        private async Task GenerateFairyTale()
        {
            if (string.IsNullOrWhiteSpace(Theme))
            {
                await ShowErrorAsync("Bitte gib ein Thema ein.");
                return;
            }

            IsGenerating = true;
            HasError = false;
            StatusMessage = "Generiere Märchen...";

            try
            {
                var request = new FairyTaleRequest
                {
                    Theme = Theme,
                    Style = SelectedStyle,
                    Mode = SelectedMode,
                    ImageCount = SelectedMode == GenerationMode.FullStory ? 4 : 0
                };

                CurrentFairyTale = await _fairyTaleService.GenerateFairyTaleAsync(request);
                StatusMessage = $"Fertig! ({CurrentFairyTale.GenerationTime.TotalSeconds:F1}s)";
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Fehler: {ex.Message}");
                CurrentFairyTale = null;
            }
            finally
            {
                IsGenerating = false;
            }
        }

        [RelayCommand]
        private async Task QuickGenerateTextOnlyAsync()
        {
            Theme = "Ein magisches Abenteuer im Wald";
            SelectedMode = GenerationMode.TextOnly;
            await GenerateFairyTaleCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task PremiumGenerateWithImagesAsync()
        {
            SelectedMode = GenerationMode.FullStory;
            await GenerateFairyTaleCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private void ClearResults()
        {
            CurrentFairyTale = null;
            StatusMessage = "Bereit";
            HasError = false;
        }

        [RelayCommand]
        private async Task ShareStoryAsync()
        {
            if (CurrentFairyTale == null) return;

            try
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = $"{CurrentFairyTale.Title}\n\n{CurrentFairyTale.Story}",
                    Title = "Mein generiertes Märchen"
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Teilen fehlgeschlagen: {ex.Message}");
            }
        }

        private async Task ShowErrorAsync(string message)
        {
            ErrorMessage = message;
            HasError = true;
            StatusMessage = "Fehler aufgetreten";

            // Optional: Dialog anzeigen
            await Application.Current.MainPage.DisplayAlert("Fehler", message, "OK");
        }
    }
}
