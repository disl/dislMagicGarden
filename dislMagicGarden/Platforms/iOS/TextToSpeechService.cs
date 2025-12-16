using AVFoundation;

namespace dislMagicGarden.Services
{
    public class TextToSpeechService : ITextToSpeechService
    {
        AVSpeechSynthesizer speechSynthesizer = new AVSpeechSynthesizer();
        AVSpeechUtterance speechUtterance = new AVSpeechUtterance();

        public Task Speak(string text, float speed)
        {
            speechSynthesizer = new AVSpeechSynthesizer();
            speechUtterance = new AVSpeechUtterance(text);

            // Setzt die Geschwindigkeit (Rate)
            // iOS nutzt Rate, Werte sind typischerweise 0.0 bis 1.0
            speechUtterance.Rate = speed;

            // Optionale Einstellungen für Sprache und Tonhöhe
            // speechUtterance.Voice = AVSpeechSynthesisVoice.FromLanguage("de-DE");
            // speechUtterance.PitchMultiplier = 1.0f; // Tonhöhe

            speechSynthesizer.SpeakUtterance(speechUtterance);

            // Hinweis: Um auf das Ende zu warten, müssten Sie einen Delegate 
            // implementieren (AVSpeechSynthesizerDelegate) und den Task dort beenden.
            return Task.CompletedTask;
        }

        public void Stop()
        {
            speechSynthesizer.PauseSpeaking(AVSpeechBoundary.Immediate);
        }
    }
}
