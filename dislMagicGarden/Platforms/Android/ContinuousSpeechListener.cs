using System;
using System.Collections.Generic;
using System.Text;

#if ANDROID
using Android.Content;
using Android.Speech;
using Android.OS;
using Android.Runtime;

namespace dislMagicGarden.Platforms.Android
{
    public class ContinuousSpeechListener : Java.Lang.Object/*, ISpeechRecognitionListener*/
    {
        private readonly Action<string> _onPartialResult;
        private readonly Action<string> _onFinalResult;
        private readonly Action _onRestartNeeded;
        private readonly Func<bool> _isListening;

        public ContinuousSpeechListener(
            Action<string> onPartialResult,
            Action<string> onFinalResult,
            Action onRestartNeeded,
            Func<bool> isListening)
        {
            _onPartialResult = onPartialResult;
            _onFinalResult = onFinalResult;
            _onRestartNeeded = onRestartNeeded;
            _isListening = isListening;
        }

        public void OnResults(Bundle results)
        {
            var matches = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
            {
                _onFinalResult?.Invoke(matches[0]);
            }

            // Wenn die App noch im Modus "Zuhören" ist, starten wir neu
            if (_isListening()) _onRestartNeeded?.Invoke();
        }

        public void OnPartialResults(Bundle partialResults)
        {
            var matches = partialResults?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
            {
                _onPartialResult?.Invoke(matches[0]);
            }
        }

        public void OnError(SpeechRecognizerError error)
        {
            // Fehler 7 (No Match) oder 6 (Timeout) ignorieren wir und starten neu,
            // falls der User nicht aktiv "Stop" gedrückt hat.
            if (_isListening())
            {
                _onRestartNeeded?.Invoke();
            }
        }

        // Diese Methoden müssen existieren, können aber leer bleiben
        public void OnReadyForSpeech(Bundle paramsBundle) { }
        public void OnBeginningOfSpeech() { }
        public void OnBufferReceived(byte[] buffer) { }
        public void OnEndOfSpeech() { }
        public void OnEvent(int eventType, Bundle @params) { }
        public void OnRmsChanged(float rmsdB) { }
    }
}

#endif