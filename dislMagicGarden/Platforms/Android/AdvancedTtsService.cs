using Android.Speech.Tts;
using dislMagicGarden.Models;
using System.Text.RegularExpressions;
using Locale = Java.Util.Locale;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace dislMagicGarden.Platforms.Android
{
    public class AndroidTtsService : ITextToSpeechService
    {
        private TextToSpeech _tts;
        private bool _isInitialized = false;
        private bool _isPaused = false;
        private bool _isSpeaking = false;

        // NEU: Benötigt für Pause/Resume
        private List<string> _sentences = new List<string>();
        private int _currentSentenceIndex = 0;

        public bool IsSpeaking => _isSpeaking;
        public bool IsPaused => _isPaused;

        public AndroidTtsService()
        {
            var context = Platform.AppContext;
            _tts = new TextToSpeech(context, new TtsInitListener(this));
        }

        private class TtsInitListener : Java.Lang.Object, TextToSpeech.IOnInitListener
        {
            private AndroidTtsService _service;

            public TtsInitListener(AndroidTtsService service)
            {
                _service = service;
            }

            public void OnInit(OperationResult status)
            {
                if (status == OperationResult.Success)
                {
                    _service._isInitialized = true;
                    _service._tts.SetLanguage(Locale.German);

                    // WICHTIG: Listener setzen, um das Ende eines Satzes zu erkennen
                    _service._tts.SetOnUtteranceProgressListener(new TtsProgressListener(_service));
                }
            }
        }

        public Task Speak(string text)
        {
            if (!_isInitialized) return Task.CompletedTask;

            // Text in Sätze aufteilen (Chunking)
            _sentences = SplitIntoSentences(text);
            _currentSentenceIndex = 0;
            _isPaused = false;

            // Sprachgeschwindigkeit hier setzen, falls benötigt
            // _tts.SetSpeechRate(_speechSpeed); 

            SpeakNextSentence();

            return Task.CompletedTask;
        }

        private void SpeakNextSentence()
        {
            if (_isPaused) return;

            if (_currentSentenceIndex < _sentences.Count)
            {
                _isSpeaking = true;
                string sentence = _sentences[_currentSentenceIndex];

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