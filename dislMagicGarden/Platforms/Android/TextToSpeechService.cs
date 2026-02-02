using Android.Content;
using Android.Runtime;
using Android.Speech.Tts;
using dislMagicGarden.Services;
using Application = Android.App.Application;
using Locale = Java.Util.Locale;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace dislMagicGarden.Platforms.Android
{
    public class TextToSpeechService : Java.Lang.Object, ITextToSpeechService, TextToSpeech.IOnInitListener
    {
        private TextToSpeech _speaker;
        private string _textToSpeak;
        private float _speed = 1.0f;
        private TaskCompletionSource<bool> _tcs;

        // Konstruktor mit explizitem Context-Parameter
        public TextToSpeechService(Context context = null)
        {
            // Verwende übergebenen Context oder Application.Context
            var appContext = context ?? Application.Context;

            _speaker = new TextToSpeech(appContext, this);
        }

        public Task Speak(string text, float speed)
        {
            _textToSpeak = text;
            _speed = speed;
            _tcs = new TaskCompletionSource<bool>();

            if (_speaker == null)
            {
                var appContext = Application.Context;
                _speaker = new TextToSpeech(appContext, this);
            }
            else
            {
                // Direkt sprechen, wenn bereits initialisiert
                StartSpeaking();
            }

            return _tcs.Task;
        }

        private void StartSpeaking()
        {
            if (_speaker != null)
            {
                _speaker.SetSpeechRate(_speed);
                _speaker.Speak(_textToSpeak, QueueMode.Flush, null, null);
                _tcs?.TrySetResult(true);
            }
        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                // Sprache einstellen
                var locale = new Locale("de", "DE");
                _speaker.SetLanguage(locale);

                // Geschwindigkeit setzen
                _speaker.SetSpeechRate(_speed);

                // Sofort sprechen, wenn Text vorhanden
                if (!string.IsNullOrEmpty(_textToSpeak))
                {
                    _speaker.Speak(_textToSpeak, QueueMode.Flush, null, null);
                }

                _tcs?.TrySetResult(true);
            }
            else
            {
                _tcs?.TrySetResult(false);
            }
        }

        public void Stop()
        {
            _speaker?.Stop();
        }

        public Task SpeakAsync(string text)
        {
            return Speak(text, _speed);
        }

        public void Pause()
        {
            Stop(); // In Android gibt es kein echtes Pause, nur Stop
        }

        public void Resume()
        {
            if (!string.IsNullOrEmpty(_textToSpeak))
            {
                _speaker?.Speak(_textToSpeak, QueueMode.Flush, null, null);
            }
        }

        public void SetRate(float rate)
        {
            _speed = rate;
            _speaker?.SetSpeechRate(rate);
        }
    }
}