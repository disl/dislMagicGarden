using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using dislMagicGarden.Helpers;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using dislMagicGarden.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace dislMagicGarden.ViewModels
{
    public partial class FairyTaleResultViewModel : BaseViewModel
    {
        public record ScrollToSentenceMessage(int Index);

        private int _currentSentenceIndex = 0;
        [ObservableProperty]
        private string[] _storyChunks;
        private CancellationTokenSource _ttsCancellation;

        public FairyTaleModel FairyTale { get; }
        private readonly Action _closeAction;
        private readonly SoundEffectService _soundEffectService;
        private readonly ITextToSpeechService _ttsService;

        const string m_c_SpeakStoryGlyphIconPlay = IconFont.Play_arrow;  //"\uE037"; //"&#xe050;";
        const string m_c_SpeakStoryGlyphIconPause = IconFont.Pause; //"\uE034"; //"&#xe1a2;";

        // Glyph Icons als Konstanten
        private const string m_c_SPEAK_ICON_PLAY = IconFont.Play_arrow;  // "\uE037";  // Play arrow
        private const string m_c_SPEAK_ICON_PAUSE = IconFont.Pause; // "\uE034"; // Pause
        private const string m_c_SPEAK_ICON_STOP = IconFont.Stop; // "\uE047";  // Stop

        // Neue Eigenschaft für das formatierte Label
        [ObservableProperty] private FormattedString _storyFormatted;

        [ObservableProperty]
        string speakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay;

        //private string _speakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;
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

        [ObservableProperty] private string textToSpeak;

        public LocaleWrapper SelectedVoice
        {
            get => _selectedVoice;
            set => SetProperty(ref _selectedVoice, value);
        }

        //public ICommand SpeakStoryCommand { get; }
        //public ICommand StopStoryCommand { get; }
        public ICommand PauseStoryCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowPictureCommand { get; }


        [RelayCommand]
        private void SpeakStory() // Synchroner Einstieg, damit der Button immer klickbar bleibt
        {
            if (_isSpeaking)
            {
                // PAUSE LOGIK
                _isSpeaking = false;
                _ttsCancellation?.Cancel();
                SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;

                // Optional: Musk pausieren (oder leise machen)
                _soundEffectService.StopBackgroundMusic();
            }
            else
            {
                // PLAY LOGIK
                _isSpeaking = true;
                SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PAUSE;

                // Hintergrundmusik starten (läuft parallel)
                _soundEffectService.SetLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                // Wir 'awaiten' das hier nicht, damit die Sprache sofort starten kann
                _ = _soundEffectService.PlayBackgroundMusicAsync("fairytail_ambient.mp3");

                // Starte den Prozess im Hintergrund, ohne den Command zu blockieren
                Task.Run(async () => await PlayNextChunk());
            }
        }

        private async Task PlayNextChunk()
        {
            if (_storyChunks == null)
            {
                _storyChunks = FairyTale.Story.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim() + ".")
                                              .ToArray();
            }

            while (_isSpeaking && _currentSentenceIndex < _storyChunks.Length)
            {
                string currentSentence = _storyChunks[_currentSentenceIndex];

                // 1. DUCKING: Musik leiser machen (z.B. auf 10%)
                _soundEffectService.SetBackgroundVolume(0.1f);


                // UI Updates müssen auf dem MainThread laufen!
                MainThread.BeginInvokeOnMainThread(() => {
                    UpdateHighlightedText(FairyTale.Story, currentSentence);
                    WeakReferenceMessenger.Default.Send(new ScrollToSentenceMessage(_currentSentenceIndex));
                });

                // Effekt triggern (Geraeusche usw.)
                await _soundEffectService.TriggerSoundForTextAsync(currentSentence);

                // Speaking starten
                _ttsCancellation = new CancellationTokenSource();

                try
                {
                    var settings = new SpeechOptions { Locale = SelectedVoice?.Locale };

                    // Warten, bis der Satz zu Ende gesprochen wurde (oder abgebrochen wurde)
                    await TextToSpeech.Default.SpeakAsync(currentSentence, settings, _ttsCancellation.Token);


                    // PAUSE ZWISCHEN SÄTZEN: Musik kurz wieder lauter (z.B. auf 40%)
                    _soundEffectService.SetBackgroundVolume(0.3f);
                    await Task.Delay(500); // Kurzes Luftholen für die Atmosphäre

                    if (_isSpeaking) _currentSentenceIndex++;
                }
                catch (OperationCanceledException)
                {
                    // Pause wurde gedrückt
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler: {ex.Message}");
                    break;
                }
            }

            if (_currentSentenceIndex >= _storyChunks.Length)
            {
                MainThread.BeginInvokeOnMainThread(() => ResetToStart());
            }
        }

        private void ResetToStart()
        {
            _isSpeaking = false;
            _currentSentenceIndex = 0;
            SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;
            // Optional: Highlighting entfernen
        }

        //[RelayCommand]
        //private async Task SpeakStory()
        //{
        //    if (_isSpeaking)
        //    {
        //        _isSpeaking = false;
        //        _ttsCancellation?.Cancel();
        //        SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;
        //        return;
        //    }

        //    _isSpeaking = true;
        //    SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PAUSE;

        //    if (StoryChunks == null)
        //    {
        //        StoryChunks = FairyTale.Story.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
        //                                      .Select(s => s.Trim() + ".")
        //                                      .ToArray();
        //    }

        //    await PlayNextChunk();
        //}

        //private async Task PlayNextChunk()
        //{
        //    while (_isSpeaking && _currentSentenceIndex < StoryChunks.Length)
        //    {
        //        string currentSentence = StoryChunks[_currentSentenceIndex];
        //        UpdateHighlightedText(FairyTale.Story, currentSentence);

        //        // Signal an die View: "Habe Text geupdated, bitte scrollen"
        //        WeakReferenceMessenger.Default.Send(new ScrollToSentenceMessage(_currentSentenceIndex));

        //        _ttsCancellation = new CancellationTokenSource();
        //        try
        //        {
        //            await TextToSpeech.Default.SpeakAsync(currentSentence, new SpeechOptions
        //            {
        //                Locale = SelectedVoice?.Locale,
        //                Pitch = 1.0f
        //            }, _ttsCancellation.Token);

        //            _currentSentenceIndex++;
        //        }
        //        catch (OperationCanceledException) { break; }
        //    }

        //    if (_currentSentenceIndex >= StoryChunks?.Length) ResetPlayback();
        //}

        private void ResetPlayback()
        {
            _isSpeaking = false;
            _currentSentenceIndex = 0;
            SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;
            // Initialen Text ohne Highlighting wiederherstellen
        }

        //private async Task PlayNextChunk()
        //{
        //    while (_isSpeaking && _currentSentenceIndex < _storyChunks.Length)
        //    {
        //        string currentSentence = _storyChunks[_currentSentenceIndex];

        //        UpdateHighlightedText(FairyTale.Story, currentSentence);

        //        // Sende eine Nachricht an die View, dass wir gescrollt werden wollen
        //        // Wir senden den Index mit, um die Position zu bestimmen
        //        WeakReferenceMessenger.Default.Send(new ScrollToSentenceMessage(_currentSentenceIndex));


        //        // Sound Effekt optional
        //        await _soundEffectService.TriggerSoundForTextAsync(currentSentence);

        //        _ttsCancellation = new CancellationTokenSource();

        //        try
        //        {
        //            var settings = new SpeechOptions
        //            {
        //                Locale = SelectedVoice?.Locale,
        //                Pitch = 1.0f,
        //                Volume = 1.0f
        //            };

        //            // Den aktuellen Chunk sprechen
        //            await TextToSpeech.Default.SpeakAsync(currentSentence, settings, _ttsCancellation.Token);

        //            _currentSentenceIndex++;
        //            await Task.Delay(100); // Kleine Pause zwischen Sätzen
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            // Wurde pausiert
        //            break;
        //        }
        //    }

        //    if (_currentSentenceIndex >= _storyChunks.Length)
        //    {
        //        // Ende der Geschichte erreicht
        //        _isSpeaking = false;
        //        _currentSentenceIndex = 0;
        //        SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;
        //        // Text-Highlighting zurücksetzen
        //        StoryFormatted = new FormattedString { Spans = { new Span { Text = FairyTale.Story, FontSize = 18 } } };
        //    }
        //}

        [RelayCommand]
        private void StopStory()
        {
            _isSpeaking = false;
            _ttsCancellation?.Cancel();
            _currentSentenceIndex = 0;
            SpeakStoryGlyphIcon = m_c_SPEAK_ICON_PLAY;

            _soundEffectService.StopBackgroundMusic();
        }


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


        public FairyTaleResultViewModel(
            FairyTaleModel fairyTale,
            Action closeAction,
            ITextToSpeechService ttsService,
            Services.SoundEffectService soundEffectService)
        {
            FairyTale = fairyTale;
            _closeAction = closeAction;

            _ttsService = ttsService;
            _soundEffectService = soundEffectService;

            SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay;

            // WICHTIG: Initiales Setzen des Textes für das Label
            StoryFormatted = new FormattedString
            {
                Spans =
                {
                    new Span
                    {
                        Text = FairyTale.Story,
                        FontSize = 18,
                        TextColor = (Color)Application.Current.Resources["MidnightBlue"]
                    }
                }
            };

            //SpeakStoryCommand = new Command(async () =>
            //{
            //    // 1. Status "Pause" prüfen -> Resume
            //    if (_ttsService.IsPaused)
            //    {
            //        _ttsService.Resume();
            //        SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPause; // Icon auf Pause stellen
            //    }
            //    // 2. Status "Playing" -> Pause
            //    else if (_ttsService.IsSpeaking)
            //    {
            //        _ttsService.Pause();
            //        SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay; // Icon auf Play stellen
            //    }
            //    // 3. Status "Stopped/Idle" -> Neu Starten
            //    else
            //    {
            //        // Geschwindigkeit laden
            //        float speed = Preferences.Get("speechSpeed", 1f);

            //        // HINWEIS: Du musst sicherstellen, dass der Service die Speed kennt.
            //        // Falls ITextToSpeechService eine SetSpeed Methode hat:
            //        // _ttsService.SetSpeed(speed); 

            //        // Oder du erweiterst die Speak-Methode im Interface:
            //        // await _ttsService.Speak(FairyTale.Story, speed);

            //        // OLD VERSION
            //        // Ambient music
            //        //_soundEffectService.SetLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            //        //await _soundEffectService.PlayBackgroundMusicAsync("fairytail_ambient.mp3");

            //        //await _ttsService.Speak(FairyTale.Story);


            //        // NEW VERSION
            //        _soundEffectService.SetLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            //        await _soundEffectService.PlayBackgroundMusicAsync("fairytail_ambient.mp3");


            //        // In deinem ViewModel oder Service

            //        // Text in Sätze aufteilen (einfache Methode)
            //        var sentences = FairyTale.Story.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
            //        var fullText = FairyTale.Story;

            //        foreach (var sentence in sentences)
            //        {
            //            if (string.IsNullOrWhiteSpace(sentence)) continue;

            //            // 1. Den aktuellen Satz inkl. Satzzeichen bauen
            //            string currentSentence = sentence.Trim() + "."; // Annahme: Punkt war im Split

            //            // --- A. VISUELL UPDATE (Highlighting) ---
            //            UpdateHighlightedText(fullText, currentSentence);

            //            // --- B. SOUND EFFEKTE ---
            //            await _soundEffectService.TriggerSoundForTextAsync(currentSentence);

            //            // --- C. TTS SPRECHEN ---
            //            // WICHTIG: Du darfst hier NICHT TextToSpeech.SpeakAsync direkt nutzen, 
            //            // wenn du Pause/Resume im _ttsService unterstützen willst.
            //            // Du musst deinen eigenen Service nutzen, der das unterstützt!

            //            // Wenn dein _ttsService nur den ganzen Text kann, ist das hier ein Problem.
            //            // Wir nehmen an, _ttsService hat eine Methode Speak(string) oder ähnlich.
            //            // Als Workaround für das Highlighting nutzen wir hier die Standard MAUI API:

            //            await TextToSpeech.SpeakAsync(currentSentence);

            //            // HINWEIS: Wenn du Pause-Resume Buttons hast, funktioniert das mit der 
            //            // Standard-Schleife hier schlecht, da man die Schleife nicht pausieren kann.
            //            // Für ein erstes "Highlighting-Demo" ist das aber okay.

            //            await Task.Delay(50);

            //        }

            //        SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPause;
            //    }
            //});

            //StopStoryCommand = new Command(() =>
            //{
            //    _ttsService.Stop();
            //    SpeakStoryGlyphIcon = m_c_SpeakStoryGlyphIconPlay; // Reset Icon
            //});


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

        private void UpdateHighlightedText(string fullText, string sentenceToHighlight)
        {
            try
            {
                // Wir suchen die Position des Satzes im Gesamtext
                int index = fullText.IndexOf(sentenceToHighlight);

                if (index >= 0)
                {
                    var formatted = new FormattedString();

                    // 1. Text VOR dem Satz (Normal)
                    if (index > 0)
                    {
                        formatted.Spans.Add(new Span
                        {
                            Text = fullText.Substring(0, index),
                            FontSize = 18,
                            TextColor = (Color)Application.Current.Resources["MidnightBlue"]
                        });
                    }

                    // 2. Der zu highlightende Satz (Fett/Bunt)
                    formatted.Spans.Add(new Span
                    {
                        Text = sentenceToHighlight,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.DarkGoldenrod // Oder deine Highlight-Farbe
                    });

                    // 3. Text NACH dem Satz (Normal)
                    int afterIndex = index + sentenceToHighlight.Length;
                    if (afterIndex < fullText.Length)
                    {
                        formatted.Spans.Add(new Span
                        {
                            Text = fullText.Substring(afterIndex),
                            FontSize = 18,
                            TextColor = (Color)Application.Current.Resources["MidnightBlue"]
                        });
                    }

                    // Zuweisen
                    StoryFormatted = formatted;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Highlighting Error: {ex.Message}");
            }
        }

        public async Task SpeakAtHalfSpeed()
        {

        }
    }

}
