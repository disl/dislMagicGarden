using Android.Runtime;
using Android.Speech.Tts;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace dislMagicGarden.Services
{
    public class TextToSpeechService : Java.Lang.Object, ITextToSpeechService, TextToSpeech.IOnInitListener
    {
        private TextToSpeech _speaker;
        private string _textToSpeak;
        private float _speed;
        private TaskCompletionSource<bool> _tcs;

        public Task Speak(string text, float speed)
        {
            _textToSpeak = text;
            _speed = speed;
            _tcs = new TaskCompletionSource<bool>();

            // Initialisiert den Android TextToSpeech-Service
            _speaker = new TextToSpeech(
                Android.App.Application.Context,
                this
            );

            return _tcs.Task;
        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                // Setzt die Geschwindigkeit (Speed/Rate)
                // Android nutzt setSpeechRate(), 1.0f ist normal
                _speaker.SetSpeechRate(_speed);

                // Startet die Sprachausgabe
                _speaker.Speak(_textToSpeak, QueueMode.Flush, null, null);

                // Optional: Sie könnten hier auf das Ende der Sprache warten, 
                // aber für dieses Beispiel beenden wir den Task sofort.
                _tcs.SetResult(true);
            }
            else
            {
                _tcs.SetResult(false);
            }
        }
    }
}
