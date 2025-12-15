using CommunityToolkit.Mvvm.ComponentModel;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace dislMagicGarden.ViewModels
{
    public partial class FairyTaleResultViewModel : BaseViewModel
    {
        public FairyTaleModel FairyTale { get; }
        private readonly Action _closeAction;

        [ObservableProperty]
        private float speakSpeed = 0.5f;

        // Verfügbare Stimmen
        public ObservableCollection<LocaleWrapper> AvailableVoices { get; } = new();
        private LocaleWrapper _selectedVoice;
        public LocaleWrapper SelectedVoice
        {
            get => _selectedVoice;
            set => SetProperty(ref _selectedVoice, value);
        }

        public ICommand SpeakStoryCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand CloseCommand { get; }

        private readonly ITextToSpeechService _ttsService;

        public async Task LoadVoicesAsync()
        {
            var locales = await TextToSpeech.GetLocalesAsync();

            AvailableVoices.Clear();

            // Aktuelle UI-Culture holen (z. B. "de-DE", "en-US")
            var currentCulture = CultureInfo.CurrentUICulture;
            string langPrefix = currentCulture.TwoLetterISOLanguageName; // z.B. "de"

            // Stimmen zuerst nach exakter Culture suchen
            var exactMatches = locales.Where(l =>
                string.Equals(l.Language, currentCulture.Name, StringComparison.OrdinalIgnoreCase));

            foreach (var match in exactMatches)
                AvailableVoices.Add(new LocaleWrapper { Locale = match });

            // Wenn keine exakte Übereinstimmung → Suche nach gleichem Sprach-Stamm ("de", "en", ...)
            if (!AvailableVoices.Any())
            {
                var fallbackMatches = locales.Where(l =>
                    l.Language.StartsWith(langPrefix, StringComparison.OrdinalIgnoreCase));

                foreach (var match in fallbackMatches)
                    AvailableVoices.Add(new LocaleWrapper { Locale = match });
            }

            // Wenn immer noch keine gefunden → einfach irgend eine nehmen
            if (!AvailableVoices.Any())
            {
                foreach (var locale in locales)
                    AvailableVoices.Add(new LocaleWrapper { Locale = locale });
            }

            SelectedVoice = AvailableVoices.FirstOrDefault();
        }


        public FairyTaleResultViewModel(FairyTaleModel fairyTale, Action closeAction, ITextToSpeechService ttsService)
        {
            FairyTale = fairyTale;
            _closeAction = closeAction;

            _ttsService = ttsService;

            SpeakStoryCommand = new Command(async () =>
            {

                await SpeakAtHalfSpeed();

                //var options = new SpeechOptions
                //{
                //    Locale = SelectedVoice?.Locale, // 💡 jetzt eindeutig
                //    Pitch = 1.15f,                   // etwas höher = märchenhaft
                //    Volume = 1.0f,                    
                //};

                //await TextToSpeech.SpeakAsync(FairyTale.Story, options);
            });

            ShareCommand = new Command(async () =>
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Title = Properties.Resources.Share_fairy_tales,
                    Text = FairyTale.Story
                });
            });

            CloseCommand = new Command(_closeAction);
        }

        public async Task SpeakAtHalfSpeed()
        {
            // Funktioniert plattformübergreifend, dank der Implementierungen
            await _ttsService.Speak(FairyTale.Story, SpeakSpeed);
        }
    }

}
