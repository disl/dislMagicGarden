
#if ANDROID
using Android.OS;
using Android.Runtime;
using Android.Speech;

namespace dislMagicGarden.Platforms.Android
{
    public class ContinuousSpeechListener : Java.Lang.Object, IRecognitionListener
    {
        private readonly Action<string> _onTextRecognized;
        private readonly Action _restartListening;

        public ContinuousSpeechListener(Action<string> onTextRecognized, Action restartListening)
        {
            _onTextRecognized = onTextRecognized;
            _restartListening = restartListening;
        }

        public void OnResults(Bundle results)
        {
            var matches = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
                _onTextRecognized(matches[0]);

            _restartListening();
        }

        public void OnPartialResults(Bundle partialResults)
        {
            var matches = partialResults?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
                _onTextRecognized(matches[0]);
        }

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            // Typische "normale" Fehler beim Continuous Listening:
            if (error == SpeechRecognizerError.NoMatch ||
                error == SpeechRecognizerError.SpeechTimeout)
            {
                _restartListening();
            }
        }

        public void OnEndOfSpeech()
        {
            _restartListening();
        }

        // Pflichtmethoden (leer)
        public void OnReadyForSpeech(Bundle @params) { }
        public void OnBeginningOfSpeech() { }
        public void OnRmsChanged(float rmsdB) { }
        public void OnBufferReceived(byte[] buffer) { }
        public void OnEvent(int eventType, Bundle @params) { }
    }

}

#endif