using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using dislMagicGarden.Models;
using System.Text.RegularExpressions;
using Debug = System.Diagnostics.Debug;
using Locale = Java.Util.Locale;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace dislMagicGarden.Platforms.Android
{
    public class AndroidTtsService : Java.Lang.Object, TextToSpeech.IOnInitListener, ITextToSpeechService  
    {
        public TextToSpeech _tts;
        private bool _isInitialized = false;
        private bool _isPaused = false;
        private bool _isSpeaking = false;

        private TtsLanguage _currentLanguage = TtsLanguage.Auto;

        private TtsGender _currentGender = TtsGender.Male;
        //private TtsGender _selectedGender = TtsGender.Male;

        private Locale _currentLocale;
        private TaskCompletionSource<bool> _initTcs;

        public bool IsInitialized => _isInitialized;


    
        private TtsLanguage _selectedLanguage = TtsLanguage.Auto;

        // NEU: Benötigt für Pause/Resume
        private List<string> _sentences = new List<string>();
        private int _currentSentenceIndex = 0;

        public bool IsSpeaking => _isSpeaking;
        public bool IsPaused => _isPaused;



        public AndroidTtsService()
        {
            var context = Platform.AppContext;

            _tts = new TextToSpeech(context, new TtsInitListener(this, _currentGender, _selectedLanguage));
        }


        private class TtsInitListener : Java.Lang.Object, TextToSpeech.IOnInitListener
        {
            private AndroidTtsService _service;
            private TtsGender _selectedGender;
            private TtsLanguage _selectedLanguage;

            public TtsInitListener(AndroidTtsService service, TtsGender selectedGender, TtsLanguage selectedLanguage)
            {
                _service = service;
                _selectedGender = selectedGender;
                _selectedLanguage = selectedLanguage;
            }

            public void OnInit(OperationResult status)
            {
                if (status == OperationResult.Success)
                {
                    _service._isInitialized = true;

                    // Standard-Sprache setzen
                    SetLocaleForLanguage(_selectedLanguage);

                    // Listener für Satzende
                    _service._tts.SetOnUtteranceProgressListener(new TtsProgressListener(_service));

                    // Stimme nach Geschlecht auswählen
                    SelectVoiceByGenderAndLanguage(_selectedGender, _selectedLanguage);
                }
            }

            private void SetLocaleForLanguage(TtsLanguage language)
            {
                try
                {
                    Locale locale = language switch
                    {
                        TtsLanguage.German => Locale.German,
                        TtsLanguage.English => Locale.English,
                        TtsLanguage.Spanish => new Locale("es"),
                        TtsLanguage.French => Locale.French,
                        TtsLanguage.Russian => new Locale("ru"),
                        TtsLanguage.Ukrainian => new Locale("uk"),
                        TtsLanguage.Auto => Locale.Default,
                        _ => Locale.Default
                    };

                    _service._tts.SetLanguage(locale);
                    System.Diagnostics.Debug.WriteLine($"Sprache gesetzt: {locale.DisplayLanguage}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fehler beim Setzen der Sprache: {ex.Message}");
                }
            }

            private Locale[] GetLocalesForLanguage(TtsLanguage language)
            {
                return language switch
                {
                    TtsLanguage.German => new[]
                    {
            Locale.German,
            Locale.Germany,
            //Locale. German Switzerland // falls verfügbar
        },
                    TtsLanguage.English => new[]
                    {
            Locale.English,
            //Locale.US,
            //Locale.UK
        },
                    TtsLanguage.Spanish => new[]
                    {
            new Locale("es", "ES"),
            new Locale("es", "MX")
        },
                    TtsLanguage.French => new[]
                    {
            Locale.French,
            new Locale("fr", "FR")
        },
                    TtsLanguage.Russian => new[]
                    {
            new Locale("ru", "RU")
        },
                    TtsLanguage.Ukrainian => new[]
                    {
            new Locale("uk", "UA")
        },
                    _ => new[] { Locale.Default }
                };
            }

            private void SelectVoiceByGenderAndLanguage(TtsGender gender, TtsLanguage language)
            {
                if (gender == TtsGender.Default || _service == null)
                    return;

                try
                {
                    // Verfügbare Stimmen abrufen (API Level 21+)
                    var availableVoices = _service._tts.Voices;

                    if (availableVoices == null || availableVoices.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Keine Stimmen verfügbar");
                        return;
                    }

                    var languageVoices = FilterVoicesByLanguage(availableVoices, language);



                    // Fallback: Wenn keine passende Sprache, nimm alle
                    if (languageVoices.Count == 0)
                    {
                        Debug.WriteLine($"Keine Stimmen für Sprache {language} gefunden, verwende alle");
                        languageVoices = availableVoices.ToList();
                    }

                    Voice selectedVoice = null;

                    // Nach Geschlecht filtern
                    foreach (var voice in languageVoices)
                    {
                        string voiceName = voice.Name.ToLowerInvariant();

                        bool isFemale = IsFemaleVoice(voiceName);
                        bool isMale = IsMaleVoice(voiceName);

                        if (gender == TtsGender.Female && isFemale)
                        {
                            selectedVoice = voice;
                            break;
                        }
                        else if (gender == TtsGender.Male && isMale)
                        {
                            selectedVoice = voice;
                            break;
                        }
                    }

                    // Fallback: Erste Stimme der gewünschten Sprache
                    if (selectedVoice == null && languageVoices.Count > 0)
                    {
                        selectedVoice = languageVoices.First(x=>x.Name.Contains("default"));
                        Debug.WriteLine($"Keine passende {gender}-Stimme, nehme erste Sprach-Stimme");
                    }

                    // Fallback: Absolute erste Stimme
                    if (selectedVoice == null && availableVoices.Count > 0)
                    {
                        selectedVoice = availableVoices.First();
                        Debug.WriteLine("Fallback: Erste verfügbare Stimme");
                    }

                    // Stimme setzen
                    if (selectedVoice != null)
                    {
                        _service._tts.SetVoice(selectedVoice);
                        Debug.WriteLine($"Stimme gesetzt: {selectedVoice.Name} ({selectedVoice.Locale})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fehler bei Stimmenauswahl: {ex.Message}");
                }
            }

            private List<Voice> FilterVoicesByLanguage(IEnumerable<Voice> voices, TtsLanguage language)
            {
                var locales = GetLocalesForLanguage(language);
                var languageCodes = GetLanguageCodes(language);

                // 1. Zuerst: Exakte Locale-Matches (Sprache + Land)
                var exactMatches = voices.Where(v =>
                    locales.Any(locale =>
                        v.Locale?.Language == locale.Language &&
                        v.Locale?.Country == locale.Country))
                    .ToList();

                if (exactMatches.Any())
                {
                    Debug.WriteLine($"Exakte Matches (mit Land): {exactMatches.Count}");
                    return exactMatches;
                }

                // 2. Dann: Nur Sprache (ohne Land)
                var languageMatches = voices.Where(v =>
                    locales.Any(locale =>
                        v.Locale?.Language == locale.Language))
                    .ToList();

                if (languageMatches.Any())
                {
                    Debug.WriteLine($"Sprach-Matches (ohne Land): {languageMatches.Count}");
                    return languageMatches;
                }

                // 3. Dann: Suche im Namen
                var nameMatches = voices.Where(v =>
                    v.Name != null &&
                    languageCodes.Any(code =>
                        v.Name.Contains($"-{code}-", StringComparison.OrdinalIgnoreCase) ||
                        v.Name.StartsWith($"{code}-", StringComparison.OrdinalIgnoreCase) ||
                        v.Name.EndsWith($"-{code}", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (nameMatches.Any())
                {
                    Debug.WriteLine($"Name-Matches: {nameMatches.Count}");
                    return nameMatches;
                }

                // 4. Fallback: Alle
                Debug.WriteLine($"Keine Matches, verwende alle {voices.Count()} Stimmen");
                return voices.ToList();
            }

            private string[] GetLanguageCodes(TtsLanguage language)
            {
                if (language == TtsLanguage.Auto)
                {
                    var defaultLocale = Locale.Default;
                    var codes = new List<string> { defaultLocale.Language };

                    // Auch die Sprache des Locale-Namens hinzufügen
                    if (!string.IsNullOrEmpty(defaultLocale.DisplayLanguage))
                    {
                        codes.Add(defaultLocale.DisplayLanguage.ToLowerInvariant());
                    }

                    Debug.WriteLine($"Auto-Sprache: {defaultLocale.Language} ({defaultLocale.DisplayLanguage})");
                    return codes.ToArray();
                }

                return language switch
                {
                    TtsLanguage.German => new[] { "de", "deu", "german", "deutsch" },
                    TtsLanguage.English => new[] { "en", "eng", "english", "englisch" },
                    TtsLanguage.Spanish => new[] { "es", "spa", "spanish", "spanisch", "espanol" },
                    TtsLanguage.French => new[] { "fr", "fra", "fre", "french", "französisch", "francais" },
                    TtsLanguage.Russian => new[] { "ru", "rus", "russian", "russisch" },
                    TtsLanguage.Ukrainian => new[] { "uk", "ukr", "ukrainian", "ukrainisch" },
                    _ => new[] { "de", "en" }
                };
            }

            private bool IsFemaleVoice(string voiceName)
            {
                if (string.IsNullOrEmpty(voiceName)) return false;

                string[] femaleKeywords = {
            // International
            "female", "woman", "girl", "f1", "f2", "f3", "f4",
            // Englisch
            "samantha", "karen", "sarah", "jennifer", "lisa", "emma", "olivia",
            "victoria", "elizabeth", "helena", "anna", "maria", "sofia",
            "alexa", "siri", "cortana",
            // Deutsch
            "weiblich", "frau", "mädchen",
            // Französisch
            "femme", "fille", "femelle", "audrey", "lea", "chloe",
            // Spanisch
            "mujer", "chica", "femenina", "sofia", "lupe",
            // Russisch
            "zhenskiy", "devushka", "yana", "katya",
            // Ukrainisch
            "zhinochiy", "divchyna", "oksana"
        };

                return femaleKeywords.Any(keyword => voiceName.Contains(keyword));
            }

            private bool IsMaleVoice(string voiceName)
            {
                if (string.IsNullOrEmpty(voiceName)) return false;

                string[] maleKeywords = {
            // International
            "male", "man", "boy", "m1", "m2", "m3", "m4",
            // Englisch
            "david", "john", "michael", "james", "robert", "william",
            "paul", "peter", "george", "thomas", "daniel", "matthew",
            "alex", "sam", "charlie",
            // Deutsch
            "männlich", "mann", "junge", "stefan", "markus",
            // Französisch
            "homme", "garçon", "masculin", "pierre", "jean",
            // Spanisch
            "hombre", "chico", "masculino", "jose", "carlos",
            // Russisch
            "muzhskoy", "malchik", "vladimir", "dmitry",
            // Ukrainisch
            "cholovichiy", "khlopchik", "oleksandr"
        };

                return maleKeywords.Any(keyword => voiceName.Contains(keyword));
            }
        }



        //private class TtsInitListener : Java.Lang.Object, TextToSpeech.IOnInitListener
        //{
        //    private AndroidTtsService _service;
        //    TtsGender _selectedGender;

        //    public TtsInitListener(AndroidTtsService service, TtsGender selectedGender)
        //    {
        //        _service = service;
        //        _selectedGender = selectedGender;
        //    }

        //    public void OnInit(OperationResult status)
        //    {
        //        if (status == OperationResult.Success)
        //        {
        //            _service._isInitialized = true;
        //            _service._tts.SetLanguage(Locale.German);

        //            // WICHTIG: Listener setzen, um das Ende eines Satzes zu erkennen
        //            _service._tts.SetOnUtteranceProgressListener(new TtsProgressListener(_service));

        //            // ?????????


        //            SelectVoiceByGender(_selectedGender);
        //        }
        //    }

        //    private void SelectVoiceByGender(TtsGender gender)
        //    {
        //        if (gender == TtsGender.Default || _service == null)
        //            return;

        //        try
        //        {
        //            // Verfügbare Stimmen abrufen (API Level 21+)
        //            var availableVoices = _service._tts.Voices;

        //            if (availableVoices == null)
        //                return;

        //            Voice selectedVoice = null;

        //            foreach (var voice in availableVoices)
        //            {
        //                string voiceName = voice.Name.ToLowerInvariant();

        //                // Deutschsprachige Stimmen bevorzugen
        //                //if (voiceName.Contains("deu") || voiceName.Contains("de-") || voiceName.Contains("german"))
        //                //{
        //                // Nach Geschlecht filtern (unzuverlässig, aber praktikabel)
        //                bool isFemale = voiceName.Contains("female") ||
        //                               voiceName.Contains("weiblich") ||
        //                               voiceName.Contains("f1") ||
        //                               voiceName.Contains("samantha") ||
        //                               voiceName.Contains("helena");

        //                bool isMale = voiceName.Contains("male") ||
        //                             voiceName.Contains("männlich") ||
        //                             voiceName.Contains("m1") ||
        //                             voiceName.Contains("david") ||
        //                             voiceName.Contains("stefan");

        //                if (gender == TtsGender.Female && isFemale)
        //                {
        //                    selectedVoice = voice;
        //                    break;
        //                }
        //                else if (gender == TtsGender.Male && isMale)
        //                {
        //                    selectedVoice = voice;
        //                    break;
        //                }
        //                //}
        //            }

        //            // Wenn gefunden, Stimme setzen
        //            if (selectedVoice != null)
        //            {
        //                _service._tts.SetVoice(selectedVoice);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine($"Fehler bei Stimmenauswahl: {ex.Message}");
        //        }
        //    }
        //}

        public Task Speak(string text, TtsLanguage? language = null, TtsGender? gender = null, float rate = 1.0f)
        {
            if (!_isInitialized)
                return Task.CompletedTask;

            // Text in Sätze aufteilen (Chunking)
            _sentences = SplitIntoSentences(text);
            _currentSentenceIndex = 0;
            _isPaused = false;

            if (!_isInitialized || _tts == null)
            {
                System.Diagnostics.Debug.WriteLine("TTS nicht initialisiert");
                return Task.CompletedTask;
            }

            try
            {
                // Temporäre Sprache/Geschlecht für diesen Satz
                if (language.HasValue || gender.HasValue)
                {
                    ApplyLanguageSettings(
                        language ?? _currentLanguage,
                        gender ?? _currentGender
                    );
                }

                // Geschwindigkeit setzen
                _tts.SetSpeechRate(rate);

                // Parameter für Utterance ID
                var parameters = new Dictionary<string, string>
                {
                    { TextToSpeech.Engine.KeyParamUtteranceId, Guid.NewGuid().ToString() }
                };

                _tts.Speak(text, QueueMode.Flush, null, Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS Speak Fehler: {ex}");

            }
            return Task.CompletedTask;

            //if (!_isInitialized) return Task.CompletedTask;

            //// Text in Sätze aufteilen (Chunking)
            //_sentences = SplitIntoSentences(text);
            //_currentSentenceIndex = 0;
            //_isPaused = false;

            //// Sprachgeschwindigkeit hier setzen, falls benötigt
            //// _tts.SetSpeechRate(_speechSpeed); 

            //SpeakNextSentence();

            //return Task.CompletedTask;
        }

        private void ApplyLanguageSettings(TtsLanguage language, TtsGender gender)
        {
            try
            {
                // Sprache auswählen
                var locale = GetLocaleForLanguage(language);
                if (locale != null)
                {
                    var result = _tts.SetLanguage(locale);

                    switch (result)
                    {
                        case LanguageAvailableResult.Available:
                            System.Diagnostics.Debug.WriteLine($"Sprache verfügbar: {locale.DisplayLanguage}");
                            _currentLocale = locale;
                            break;
                        case LanguageAvailableResult.MissingData:
                            System.Diagnostics.Debug.WriteLine($"Sprachdaten fehlen für: {locale.DisplayLanguage}");
                            // Intent zum Installieren öffnen
                            InstallLanguageData(locale);
                            break;
                        case LanguageAvailableResult.NotSupported:
                            System.Diagnostics.Debug.WriteLine($"Sprache nicht unterstützt: {locale.DisplayLanguage}");
                            break;
                    }
                }

                // Stimme nach Geschlecht auswählen (API 21+)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    SelectVoiceByGender(gender);
                }

                _currentLanguage = language;
                _currentGender = gender;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Anwenden der Spracheinstellungen: {ex}");
            }
        }

        private Locale GetLocaleForLanguage(TtsLanguage language)
        {
            if (language == TtsLanguage.Auto)
            {
                return Locale.Default;
            }

            return language switch
            {
                TtsLanguage.German => Locale.German,
                TtsLanguage.English => Locale.English,
                TtsLanguage.Spanish => new Locale("es", "ES"),
                TtsLanguage.French => Locale.French,
                TtsLanguage.Russian => new Locale("ru", "RU"),
                TtsLanguage.Ukrainian => new Locale("uk", "UA"),
                _ => Locale.Default
            };
        }

        private void SelectVoiceByGender(TtsGender gender)
        {
            try
            {
                var voices = _tts.Voices;
                if (voices == null || voices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Keine Stimmen verfügbar");
                    return;
                }

                // Verfügbare Stimmen anzeigen
                System.Diagnostics.Debug.WriteLine($"Verfügbare Stimmen ({voices.Count}):");
                foreach (var v in voices)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {v.Name} ({v.Locale})");
                }

                // Stimmen für aktuelle Sprache
                var languageVoices = voices.Where(v =>
                    v.Locale?.Language == _currentLocale.Language).ToList();

                if (languageVoices.Count == 0)
                {
                    languageVoices = voices.ToList();
                }

                Voice selectedVoice = null;

                if (gender == TtsGender.Female)
                {
                    selectedVoice = languageVoices.FirstOrDefault(v => IsFemaleVoice(v.Name));
                }
                else if (gender == TtsGender.Male)
                {
                    selectedVoice = languageVoices.FirstOrDefault(v => IsMaleVoice(v.Name));
                }

                // Fallback: Erste Stimme
                selectedVoice ??= languageVoices.FirstOrDefault() ?? voices.First();

                if (selectedVoice != null)
                {
                    _tts.SetVoice(selectedVoice);
                    System.Diagnostics.Debug.WriteLine($"Stimme gesetzt: {selectedVoice.Name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler bei Stimmenauswahl: {ex}");
            }
        }

        private bool IsFemaleVoice(string voiceName)
        {
            if (string.IsNullOrEmpty(voiceName)) return false;

            var name = voiceName.ToLowerInvariant();
            string[] femaleKeywords = {
                "female", "weiblich", "f1", "f2", "f3",
                "samantha", "helena", "victoria", "karen", "sarah",
                "zhenskiy", "zhinochiy", "devushka", "divchyna",
                "femme", "mujer", "donna", "女性", "여성"
            };

            return femaleKeywords.Any(k => name.Contains(k));
        }

        private bool IsMaleVoice(string voiceName)
        {
            if (string.IsNullOrEmpty(voiceName)) return false;

            var name = voiceName.ToLowerInvariant();
            string[] maleKeywords = {
                "male", "männlich", "m1", "m2", "m3",
                "david", "stefan", "paul", "john", "michael",
                "muzhskoy", "cholovichiy", "malchik", "khlopchik",
                "homme", "hombre", "uomo", "男性", "남성"
            };

            return maleKeywords.Any(k => name.Contains(k));
        }

        private void InstallLanguageData(Locale locale)
        {
            try
            {
                var intent = new Intent();
                intent.SetAction(TextToSpeech.Engine.ActionInstallTtsData);
                intent.AddFlags(ActivityFlags.NewTask);
                Platform.CurrentActivity?.StartActivity(intent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Öffnen des Installationsdialogs: {ex}");
            }
        }

        public void SetLanguage(TtsLanguage language)
        {
            if (!_isInitialized) return;
            ApplyLanguageSettings(language, _currentGender);
        }

        public void SetGender(TtsGender gender)
        {
            if (!_isInitialized) return;
            ApplyLanguageSettings(_currentLanguage, gender);
        }

        public TtsLanguage GetCurrentLanguage() => _currentLanguage;
        public TtsGender GetCurrentGender() => _currentGender;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tts?.Stop();
                _tts?.Shutdown();
                _tts?.Dispose();
                _tts = null;
            }
            base.Dispose(disposing);
        }

        private void SpeakNextSentence()
        {
            if (_isPaused) return;

            if (_currentSentenceIndex < _sentences.Count)
            {
                _isSpeaking = true;
                string sentence = _sentences[_currentSentenceIndex];

                _tts.SetSpeechRate(0.9f);

                // Parameter: Text, QueueMode, BundleParams, UtteranceId
                _tts.Speak(sentence, QueueMode.Flush, null, Guid.NewGuid().ToString());
            }
            else
            {
                // Alles gesprochen
                _isSpeaking = false;
                _isPaused = false;
                _currentSentenceIndex = 0;
            }
        }

        // Hilfsmethode zum Aufteilen des Textes
        private List<string> SplitIntoSentences(string text)
        {
            // Teilt an Satzenden (. ! ?) und behaltet die Trennzeichen
            return Regex.Split(text, @"(?<=[\.!\?])")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
        }

        public void Pause()
        {
            if (_tts != null && _isSpeaking)
            {
                _tts.Stop(); // Bricht den aktuellen Satz ab
                _isPaused = true;
                _isSpeaking = false; // UI-Status aktualisieren
            }
        }

        public void Resume()
        {
            if (_isPaused)
            {
                _isPaused = false;
                _isSpeaking = true;
                // Spricht den aktuellen Satz (an dem wir stehen geblieben sind) weiter
                SpeakNextSentence();
            }
        }

        public void Stop()
        {
            if (_tts != null)
            {
                _tts.Stop();
                _isSpeaking = false;
                _isPaused = false;
                _currentSentenceIndex = 0; // Reset
            }
        }

        public async Task InitializeAsync()
        {
            _initTcs = new TaskCompletionSource<bool>();

            try
            {
                var context = Platform.CurrentActivity;
                _tts = new TextToSpeech(context, this);

                // 5 Sekunden Timeout für Initialisierung
                await Task.WhenAny(_initTcs.Task, Task.Delay(5000));

                if (!_isInitialized)
                {
                    System.Diagnostics.Debug.WriteLine("TTS Initialisierung timeout");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS Init Fehler: {ex}");
                _initTcs?.TrySetException(ex);
            }
        }

        //public Task Speak(string text, Models.TtsLanguage? language = null, Models.TtsGender? gender = null, float rate = 1)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetLanguage(Models.TtsLanguage language)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetGender(Models.TtsGender gender)
        //{
        //    throw new NotImplementedException();
        //}

        //public Models.TtsLanguage GetCurrentLanguage()
        //{
        //    throw new NotImplementedException();
        //}

        //public Models.TtsGender GetCurrentGender()
        //{
        //    throw new NotImplementedException();
        //}

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                _isInitialized = true;

                // Systemsprache erkennen
                _currentLocale = Locale.Default;
                System.Diagnostics.Debug.WriteLine($"Systemsprache: {_currentLocale.DisplayLanguage} ({_currentLocale.Language})");

                // Standardmäßig Auto (Systemsprache)
                ApplyLanguageSettings(_currentLanguage, _currentGender);

                _initTcs?.TrySetResult(true);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"TTS Init fehlgeschlagen: {status}");
                _initTcs?.TrySetException(new Exception($"TTS Init fehlgeschlagen: {status}"));
            }
        }

        // Listener Implementierung
        private class TtsProgressListener : UtteranceProgressListener
        {
            private AndroidTtsService _service;

            public TtsProgressListener(AndroidTtsService service)
            {
                _service = service;
            }

            public override void OnDone(string utteranceId)
            {
                // Ein Satz ist fertig -> Nächsten starten
                _service._currentSentenceIndex++;
                _service.SpeakNextSentence();
            }

            public override void OnError(string utteranceId)
            {
                _service._isSpeaking = false;
            }

            public override void OnStart(string utteranceId) { }
        }
    }
}