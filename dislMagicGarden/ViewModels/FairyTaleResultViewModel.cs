using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        const string m_c_SpeakStoryGlyphIconPlay = "\uE037"; //"&#xe050;";
        const string m_c_SpeakStoryGlyphIconPause = "\uE034"; //"&#xe1a2;";

        // Glyph Icons als Konstanten
        private const string m_c_SPEAK_ICON_PLAY = "\uE037";  // Play arrow
        private const string m_c_SPEAK_ICON_PAUSE = "\uE034"; // Pause
        private const string m_c_SPEAK_ICON_STOP = "\uE047";  // Stop

        [ObservableProperty]
        string speakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay;

        private string _speakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;
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

        [RelayCommand]
        private async void StartQuiz()
        {
            // Angenommen, _currentStoryResponse enthält die Liste der Fragen
            List<QuizQuestion> questions = FairyTale.QuizQuestions;

            //// Navigation zur QuizPage und Übergabe der Daten
            //// Variante A (direkt und einfach):
            //var quizPage = new QuizPage(new QuizViewModel());
            //quizPage.SetQuizData(questions);

            //await Application.Current.MainPage.Navigation.PushAsync(quizPage);

            // Variante B (Shell Navigation mit Parametern - sauberer MVVM Weg):
            var navigationParameter = new Dictionary<string, object>
             {
                 { "Questions", questions }
             };
            await Shell.Current.GoToAsync(nameof(QuizPage), navigationParameter);
        }

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

            SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay;

            //SpeakStoryCommand = new Command(async () =>
            //{
            //    //if (SpeakStoryGlyphIcon == m_c_SpeakStoryGlyphIconPlay)
            //    if (!_ttsService.IsSpeaking)
            //    {
            //        SpeechSpeed = Preferences.Get("speechSpeed", 1f);

            //        await _ttsService.Speak(FairyTale.Story);
            //    }
            //    else
            //    {
            //        _ttsService.Pause();
            //    }

            //    SpeakStoryGlyphIcon = SpeakStoryGlyphIcon == m_c_SpeakStoryGlyphIconPlay ? m_c_SpeakStoryGlyphIconPause : m_c_SpeakStoryGlyphIconPlay;
            //});

            SpeakStoryCommand = new Command(async () =>
            {
                // 1. Status "Pause" prüfen -> Resume
                if (_ttsService.IsPaused)
                {
                    _ttsService.Resume();
                    SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPause; // Icon auf Pause stellen
                }
                // 2. Status "Playing" -> Pause
                else if (_ttsService.IsSpeaking)
                {
                    _ttsService.Pause();
                    SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay; // Icon auf Play stellen
                }
                // 3. Status "Stopped/Idle" -> Neu Starten
                else
                {
                    // Geschwindigkeit laden
                    float speed = Preferences.Get("speechSpeed", 1f);

                    // HINWEIS: Du musst sicherstellen, dass der Service die Speed kennt.
                    // Falls ITextToSpeechService eine SetSpeed Methode hat:
                    // _ttsService.SetSpeed(speed); 

                    // Oder du erweiterst die Speak-Methode im Interface:
                    // await _ttsService.Speak(FairyTale.Story, speed);

                    await _ttsService.Speak(FairyTale.Story);
                    SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPause;
                }
            });

            StopStoryCommand = new Command(() =>
            {
                _ttsService.Stop();
                SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay; // Reset Icon
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
            });

            CloseCommand = new Command(_closeAction);
        }

        public async Task SpeakAtHalfSpeed()
        {

        }
    }

}
