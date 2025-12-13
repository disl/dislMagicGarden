using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using dislMagicGarden.Properties;
using dislMagicGarden.Services;
using dislMagicGarden.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace dislMagicGarden.ViewModels
{

    public partial class FairyTaleViewModel : BaseViewModel
    {
        private readonly IHybridFairyTaleService _fairyTaleService;

        [ObservableProperty]
        private string _theme = ""; // "Ein kleiner Drache lernt fliegen";

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

        // Verfügbare Märchentypen
        public ObservableCollection<FairyTaleTypeOption> AvailableFairyTaleTypes { get; } = new(FairyTaleTypes.All);

        [ObservableProperty]
        FairyTaleTypeOption? selectedFairyTaleType;

        //public FairyTaleTypeOption? SelectedFairyTaleType
        //{
        //    get => _selectedFairyTaleType;
        //    set => SetProperty(ref _selectedFairyTaleType, value);
        //}



        // Verfügbare Sprachen

        public ObservableCollection<LanguageOption> AvailableLanguages { get; }
         = new ObservableCollection<LanguageOption>
             {
                new() { Code = "en-US", DisplayName = "English (US)" },
                new() { Code = "de-DE", DisplayName = "Deutsch (DE)" },
                new() { Code = "fr-FR", DisplayName = "Français (FR)" },
                new() { Code = "es-ES", DisplayName = "Español (ES)" },
                new() { Code = "it-IT", DisplayName = "Italiano (IT)" },
                new() { Code = "ru-RU", DisplayName = "Русский (RU)" }
             };


        private bool _isApplyingLanguage;

        private LanguageOption _selectedLanguage;
        public LanguageOption SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (value == null)
                    return;

                if (_selectedLanguage?.Code == value.Code)
                    return;

                SetProperty(ref _selectedLanguage, value);

                // verhindert Re-Entry beim Reload / Init
                if (_isApplyingLanguage)
                    return;

                ApplyLanguage(value.Code);
            }
        }

        public FairyTaleViewModel()
        {
            // Default Gerätelanguage übernehmen
            var currentCulture = CultureInfo.CurrentUICulture.Name;

            SelectedLanguage =
                AvailableLanguages.FirstOrDefault(l => l.Code == currentCulture)
                ?? AvailableLanguages.First(l => l.Code.StartsWith("en"));

            SelectedFairyTaleType = AvailableFairyTaleTypes.FirstOrDefault();
        }

        //partial void OnSelectedLanguageChanged(string value)
        //{
        //    if (!string.IsNullOrWhiteSpace(value))
        //    {
        //        ApplyLanguage(value);
        //    }
        //}

        //private void ApplyLanguage(string lang)
        //{
        //    try
        //    {
        //        var culture = new CultureInfo(lang);

        //        CultureInfo.DefaultThreadCurrentCulture = culture;
        //        CultureInfo.DefaultThreadCurrentUICulture = culture;

        //        Thread.CurrentThread.CurrentCulture = culture;
        //        Thread.CurrentThread.CurrentUICulture = culture;

        //        // Sprache speichern
        //        Preferences.Set("AppLanguage", lang);

        //        // andere ViewModels informieren
        //        //WeakReferenceMessenger.Default.Send(
        //        //    new LanguageChangedMessage(lang));
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Language change failed: " + ex.Message);
        //    }
        //}

        private void ApplyLanguage(string lang)
        {
            if (CultureInfo.CurrentUICulture.Name == lang)
                return;

            try
            {
                _isApplyingLanguage = true;

                LanguageService.SetLanguage(lang);
                Preferences.Set("app_language", lang);

                // optional: TTS Stimmen neu laden
                //_ = LoadVoicesAsync();

                App.Reload();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Language change failed: " + ex.Message);
            }
            finally
            {
                _isApplyingLanguage = false;
            }
        }



        // Duration-Auswahl (in Minuten)
        public ObservableCollection<int> DurationOptions { get; } = new() { 3, 5, 10 };
        private int _selectedDuration = 3;
        public int SelectedDuration
        {
            get => _selectedDuration;
            set => SetProperty(ref _selectedDuration, value);
        }

        public FairyTaleViewModel(IHybridFairyTaleService fairyTaleService, ILanguageService language)
        {
            _fairyTaleService = fairyTaleService;
            Title = Properties.Resources.Home_NewStory;
            _language = language;

            Moods = new()
            {
                Resources.Reassuring,
                Resources.Adventurous,
                Resources.Funny
            };

            // Default Gerätelanguage übernehmen
            var currentCulture = CultureInfo.CurrentUICulture.Name;

            _isApplyingLanguage = true;

            SelectedLanguage =
                AvailableLanguages.FirstOrDefault(l => l.Code == currentCulture)
                ?? AvailableLanguages.First(l => l.Code.StartsWith("en"));

            _isApplyingLanguage = false;

            SelectedFairyTaleType = AvailableFairyTaleTypes.FirstOrDefault();
        }

        [RelayCommand]
        private async Task GenerateFairyTale()
        {
            if (string.IsNullOrWhiteSpace(Theme))
            {
                await ShowErrorAsync(Properties.Resources.Please_enter_a_topic);
                return;
            }

            IsGenerating = true;
            HasError = false;
            StatusMessage = $"{Properties.Resources.Generate_fairy_tales}...";

            try
            {
                var request = new FairyTaleRequest
                {
                    Theme = Theme,
                    Style = SelectedStyle,
                    Mode = SelectedMode,
                    ImageCount = SelectedMode == GenerationMode.FullStory ? 4 : 0,
                    Duration_min = SelectedDuration,
                    FairyTaleType = SelectedFairyTaleType?.Type ?? FairyTaleType.Funny,
                };

                CurrentFairyTale = await _fairyTaleService.GenerateFairyTaleAsync(request);

                if (CurrentFairyTale != null)
                {
                    var model = ConvertResponseToModel(CurrentFairyTale);

                    await Application.Current.MainPage.Navigation
                        .PushModalAsync(new FairyTaleResultPage(model), true);
                }


                StatusMessage = $"Fertig! ({CurrentFairyTale.GenerationTime.TotalSeconds:F1}s)";
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"{Properties.Resources.Error}: {ex.Message}");
                CurrentFairyTale = null;
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private FairyTaleModel ConvertResponseToModel(FairyTaleResponse response)
        {
            if (response == null)
                return null;

            return new FairyTaleModel
            {
                Title = response.Title,
                Story = response.Story,
                Moral = response.Moral,
                Characters = new ObservableCollection<string>(response.Characters),
                ImageUrls = new ObservableCollection<string>(response.ImageUrls),
                ImagePromptsCombined = string.Join("\n\n", response.ImagePrompts),

                CostText = response.Cost != null
                    ? $"{Properties.Resources.Costs}: ${response.Cost?.TotalCost:F4}"
                    : string.Empty,

                DurationText = response.GenerationTime.TotalSeconds > 0
                    ? $"{Properties.Resources.Duration}: {response.GenerationTime.TotalSeconds:F1} {Properties.Resources.seconds}"
                    : string.Empty
            };
        }


        [RelayCommand]
        private async Task QuickGenerateTextOnlyAsync()
        {
            //Theme = ""; // Ein magisches Abenteuer im Wald";
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
            StatusMessage = Properties.Resources.Ready;
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
                    Title = "Share file"
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"{Properties.Resources.Error}: {ex.Message}");
            }
        }

        private async Task ShowErrorAsync(string message)
        {
            ErrorMessage = message;
            HasError = true;
            StatusMessage = Properties.Resources.Error;

            // Optional: Dialog anzeigen
            await Application.Current.MainPage.DisplayAlert(Properties.Resources.Error, message, "OK");
        }
    }
}
