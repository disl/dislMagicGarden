using Android.Speech.Tts;
using dislMagicGarden.Models;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace dislMagicGarden.Platforms.Android
{
    // Konkrete Implementierung von UtteranceProgressListener
    public class AndroidTtsService : ITextToSpeechService
    {
        private TextToSpeech _tts;
        private bool _isInitialized = false;
        private bool _isPaused = false;
        private bool _isSpeaking = false;

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
                    _service._tts.SetLanguage(Java.Util.Locale.German);
                    _service._tts.SetSpeechRate(1.0f);
                }
            }
        }

        public async Task Speak(string text)
        {
            if (!_isInitialized) return;

            _isSpeaking = true;
            _isPaused = false;

            await Task.Run(() =>
            {
                _tts.Speak(text, QueueMode.Flush, null, Guid.NewGuid().ToString());
            });

            // Simuliere "OnDone" - in echtem Code würdest du einen Listener verwenden
            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                _isSpeaking = false;
                return false;
            });
        }

        public void Pause()
        {
            //if (_tts != null && _isSpeaking)
            if ( _isSpeaking)
            {
                _tts.Stop();
                _isPaused = true;
                _isSpeaking = false;
            }
        }

        public void Resume()
        {
            if (_isPaused)
            {
                // Hier musst du den Rest des Textes speichern und fortsetzen
                _isPaused = false;
                _isSpeaking = true;
            }
        }

        public void Stop()
        {
            if (_tts != null)
            {
                _tts.Stop();
                _isSpeaking = false;
                _isPaused = false;
            }
        }
    }
}
