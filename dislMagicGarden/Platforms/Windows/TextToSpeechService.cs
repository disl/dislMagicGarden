using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace dislMagicGarden.Services
{
    public class TextToSpeechService : ITextToSpeechService
    {
        // Der Synthesizer generiert den Audio-Stream
        private readonly SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        // Der MediaPlayer ist der moderne Ansatz für die Audio-Wiedergabe
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        public async Task Speak(string text, float speed)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // --- 1. Geschwindigkeit über SSML steuern ---

            // Die Windows-API steuert die Rate über SSML (Speech Synthesis Markup Language).
            // Die Rate wird als Prozentsatz relativ zur Normalrate (100%) angegeben.
            // Beispiel: speed=0.5f entspricht "50%", speed=1.5f entspricht "150%".
            int ssmlRate = (int)(speed * 100);

            // Erstellen des vollständigen SSML-Strings:
            string ssmlText = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='de-DE'>" +
                              $"<prosody rate='{ssmlRate}%'>{text}</prosody>" +
                              $"</speak>";

            try
            {
                // 2. Synthetisiere den SSML-Text zu einem Audio-Stream
                SpeechSynthesisStream stream = await _speechSynthesizer.SynthesizeSsmlToStreamAsync(ssmlText);

                // 3. Setze den Stream als Quelle für den MediaPlayer
                _mediaPlayer.Source = MediaSource.CreateFromStream(stream, stream.ContentType);

                // 4. Erstellen eines TaskCompletionSource, um auf das Ende der Wiedergabe zu warten
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                // Ereignishandler für das Ende der Wiedergabe. 
                // Das PlaybackSession.PlaybackStateChanged-Ereignis ist der robusteste Weg,
                // um den Abschluss der Wiedergabe zu erkennen.
                _mediaPlayer.PlaybackSession.PlaybackStateChanged += (sender, args) =>
                {
                    if (sender.PlaybackState == MediaPlaybackState.Paused)
                    {
                        // Stoppen bedeutet, die Wiedergabe ist abgeschlossen oder wurde beendet.
                        tcs.TrySetResult(true);
                    }
                };

                // 5. Starte die Wiedergabe
                _mediaPlayer.Play();

                // 6. Warte, bis die Wiedergabe beendet ist
                await tcs.Task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TextToSpeech Windows Error: {ex.Message}");
                // Falls ein Fehler auftritt, den Task abschließen
                throw;
            }
        }

        public void Stop()
        {
            _mediaPlayer.Pause();
        }
    }
}