using AVFoundation;

namespace dislMagicGarden.Services
{
    public class TextToSpeechService : ITextToSpeechService
    {
        // Der Synthesizer ist die zentrale Klasse in AVFoundation
        private readonly AVSpeechSynthesizer _speechSynthesizer = new AVSpeechSynthesizer();

        public Task Speak(string text, float speed)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.CompletedTask;

            var speechUtterance = new AVSpeechUtterance(text);

            // Setzt die Geschwindigkeit (Rate)
            // Die Rate auf iOS/MacCatalyst wird über Utterance.Rate gesteuert.
            // Der Wertbereich liegt üblicherweise zwischen AVSpeechUtterance.MinimumSpeechRate (0.0) 
            // und AVSpeechUtterance.MaximumSpeechRate (1.0).
            // Normalgeschwindigkeit (Default) ist AVSpeechUtterance.DefaultSpeechRate (oft 0.5)

            // Um eine einfache Steuerung (0.0 bis 1.0) zu gewährleisten, können wir den Wert direkt zuweisen:
            // Wenn der Benutzer 0.5f übergibt, sprechen wir halb so schnell wie das Maximum.

            // Alternativ können Sie "speed" auf den Bereich 0.0 - 1.0 mappen, wobei 0.5 normal ist.
            // Hier verwenden wir den Wert direkt als Rate-Multiplikator:

            // WICHTIG: Die Standardrate (DefaultSpeechRate) von AVSpeechUtterance ist 0.5f. 
            // Wenn Sie 1.0f (volle Geschwindigkeit) wünschen, ist das oft das Maximum.

            // Hier verwenden wir einen einfachen Multiplikator:
            speechUtterance.Rate = speed;

            // Optionale Einstellungen (z.B. Sprache auf Deutsch)
            // speechUtterance.Voice = AVSpeechSynthesisVoice.FromLanguage("de-DE");

            // Die Tonhöhe kann ebenfalls eingestellt werden (Pitch)
            // speechUtterance.PitchMultiplier = 1.0f; 

            _speechSynthesizer.SpeakUtterance(speechUtterance);

            // HINWEIS: Um auf das Ende der Sprachausgabe zu warten, müssten Sie einen Delegate
            // (AVSpeechSynthesizerDelegate) verwenden und einen TaskCompletionSource
            // implementieren. Für eine einfache "Feuer-und-Vergiss"-Implementierung reicht Task.CompletedTask.
            return Task.CompletedTask;
        }

        public void Stop()
        {
            _speechSynthesizer.PauseSpeaking(AVSpeechBoundary.Immediate);
        }
    }
}
