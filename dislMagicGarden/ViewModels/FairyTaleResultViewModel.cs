using CommunityToolkit.Mvvm.ComponentModel;
using dislMagicGarden.Models;
using dislMagicGarden.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace dislMagicGarden.ViewModels
{
    public partial class FairyTaleResultViewModel : BaseViewModel
    {
        public FairyTaleModel FairyTale { get; }
        private readonly Action _closeAction;

       const string  SpeakStoryGlyphIconPlay = "\uE037"; //"&#xe050;";
       const string SpeakStoryGlyphIconPause = "\uE034"; //"&#xe1a2;";

        // Glyph Icons als Konstanten
        private const string SPEAK_ICON_PLAY = "\uE037";  // Play arrow
        private const string SPEAK_ICON_PAUSE = "\uE034"; // Pause
        private const string SPEAK_ICON_STOP = "\uE047";  // Stop

        [ObservableProperty]
        string speakStoryGlyphIcon= SpeakStoryGlyphIconPlay;

        private string _speakStoryGlyphIcon = SPEAK_ICON_PLAY;
        private bool _isSpeaking = false;

      

        [ObservableProperty]
        private float speechSpeed = 1f;

        partial void OnSpeechSpeedChanging(float value)
        {
            Preferences.Set("speechSpeed", value);
        }

        // Verfügbare Stimmen
        public ObservableCollection<LocaleWrapper> AvailableVoices { get; } = new();
        private LocaleWrapper _selectedVoice;
        public LocaleWrapper SelectedVoice
        {
            get => _selectedVoice;
            set => SetProperty(ref _selectedVoice, value);
        }

        public ICommand SpeakStoryCommand { get; }
        public ICommand StopStoryCommand { get; }
        public ICommand PauseStoryCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowPictureCommand { get; }

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

            SpeakStoryGlyphIcon = SpeakStoryGlyphIconPlay;

            SpeakStoryCommand = new Command(async () =>
            {
                if (SpeakStoryGlyphIcon == SpeakStoryGlyphIconPlay)
                {
                    SpeechSpeed = Preferences.Get("speechSpeed", 1f);

                    await _ttsService.Speak(FairyTale.Story);
                }
                else
                {
                    _ttsService.Pause();
                }

                SpeakStoryGlyphIcon = SpeakStoryGlyphIcon == SpeakStoryGlyphIconPlay ? SpeakStoryGlyphIconPause : SpeakStoryGlyphIconPlay;



                //var options = new SpeechOptions
                //{
                //    Locale = SelectedVoice?.Locale, // 💡 jetzt eindeutig
                //    Pitch = 1.15f,                   // etwas höher = märchenhaft
                //    Volume = 1.0f,                    
                //};

                //await TextToSpeech.SpeakAsync(FairyTale.Story, options);
            });

            StopStoryCommand = new Command(() =>
            {
                _ttsService.Stop();
            });

            PauseStoryCommand = new Command(() =>
            {
                _ttsService.Pause();
            });

            ShareCommand = new Command(async () =>
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Title = Properties.Resources.Share_fairy_tales,
                    Text = FairyTale.Story
                });
            });

            ShowPictureCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation
                       .PushModalAsync(new ColoringGenerator(FairyTale.Story, FairyTale.Title), true);

                //string encodedPrompt = Uri.EscapeDataString(Theme);
                //await Shell.Current.GoToAsync($"{nameof(ColoringGenerator)}?Prompt={encodedPrompt}");
            });

            CloseCommand = new Command(_closeAction);
        }

        public async Task SpeakAtHalfSpeed()
        {
           
        }
    }

}
