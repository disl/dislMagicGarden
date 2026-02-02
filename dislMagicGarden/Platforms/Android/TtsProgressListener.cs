using Android.Content;
using Android.OS;
using Android.Speech.Tts;
using global::Android.Runtime;
using Math = System.Math;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace dislMagicGarden.Platforms.Android
{
    public class PausableTextToSpeech : Java.Lang.Object, TextToSpeech.IOnInitListener, IDisposable
    {
        private TextToSpeech _tts;
        private TtsUtteranceListener _listener;
        private Queue<string> _textQueue = new Queue<string>();
        private bool _isInitialized = false;
        private bool _isPaused = false;
        private bool _isSpeaking = false;
        private string _currentUtteranceId;
        private TaskCompletionSource<bool> _speakCompletionSource;

        public PausableTextToSpeech(Context context)
        {
            _tts = new TextToSpeech(context, this);
            _listener = new TtsUtteranceListener();

            _listener.OnChunkCompleted += (s, id) =>
            {
                if (_currentUtteranceId == id)
                {
                    _speakCompletionSource?.TrySetResult(true);
                }
            };
        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                _isInitialized = true;
                _tts.SetOnUtteranceProgressListener(_listener);

                // Sprache einstellen (z.B. Deutsch)
                var locale = new Java.Util.Locale("de", "DE");
                var result = _tts.SetLanguage(locale);

                if (result == LanguageAvailableResult.MissingData ||
                    result == LanguageAvailableResult.NotSupported)
                {
                    // Fallback auf Default
                    _tts.SetLanguage(Java.Util.Locale.Default);
                }
            }
        }

        public async Task SpeakAsync(string text, bool queue = false)
        {
            if (!_isInitialized || string.IsNullOrEmpty(text))
                return;

            await Task.Delay(100); // Kurze Verzögerung für Initialisierung

            // Text in Chunks aufteilen
            var chunks = SplitTextIntoChunks(text);

            foreach (var chunk in chunks)
            {
                if (_isPaused)
                {
                    // Warte bis Pause aufgehoben wird
                    await WaitWhilePausedAsync();
                }

                if (!_isSpeaking)
                    break;

                await SpeakChunkAsync(chunk);
            }
        }

        private async Task SpeakChunkAsync(string chunkText)
        {
            _speakCompletionSource = new TaskCompletionSource<bool>();
            _currentUtteranceId = Guid.NewGuid().ToString();

            // Parameter für Utterance ID
            var parameters = new Bundle();
            parameters.PutString(TextToSpeech.Engine.KeyParamUtteranceId, _currentUtteranceId);

            // QueueMode: Add = zur Queue hinzufügen, Flush = Queue leeren und sofort sprechen
            var queueMode = QueueMode.Flush;

            // Speak aufrufen - korrekte Parameter-Reihenfolge
            _tts.Speak(
                chunkText,      // text
                queueMode,      // queueMode
                parameters,     // params
                _currentUtteranceId // utteranceId
            );

            // Warte auf Completion (durch Listener)
            await _speakCompletionSource.Task;
        }

        private async Task WaitWhilePausedAsync()
        {
            while (_isPaused && _isSpeaking)
            {
                await Task.Delay(100);
            }
        }

        public void Pause()
        {
            _isPaused = true;
            if (_isSpeaking)
            {
                _tts.Stop();
            }
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public void Stop()
        {
            _isSpeaking = false;
            _isPaused = false;
            _tts.Stop();
            _textQueue.Clear();
            _speakCompletionSource?.TrySetCanceled();
        }

        private List<string> SplitTextIntoChunks(string text, int maxChunkLength = 200)
        {
            var chunks = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            int index = 0;
            while (index < text.Length)
            {
                int length = Math.Min(maxChunkLength, text.Length - index);

                if (index + length < text.Length)
                {
                    // Versuche bei Satzende zu schneiden
                    int lastSentenceEnd = text.LastIndexOfAny(new[] { '.', '!', '?', ';' },
                        index + length - 1, Math.Min(50, length));

                    if (lastSentenceEnd > index)
                    {
                        length = lastSentenceEnd - index + 1;
                    }
                    else
                    {
                        // Sonst bei Wortende
                        int lastSpace = text.LastIndexOf(' ', index + length - 1, Math.Min(30, length));
                        if (lastSpace > index)
                        {
                            length = lastSpace - index + 1;
                        }
                    }
                }

                chunks.Add(text.Substring(index, length).Trim());
                index += length;
            }

            return chunks;
        }

        public void SetSpeechRate(float rate)
        {
            if (_isInitialized)
            {
                _tts.SetSpeechRate(rate); // 0.5f - 2.0f
            }
        }

        public void SetPitch(float pitch)
        {
            if (_isInitialized)
            {
                _tts.SetPitch(pitch); // 0.5f - 2.0f
            }
        }

        public new void Dispose()
        {
            Stop();
            _listener?.Dispose();
            _tts?.Stop();
            _tts?.Shutdown();
            _tts?.Dispose();
            base.Dispose();
        }
    }
}




// Custom UtteranceProgressListener
public class TtsUtteranceListener : UtteranceProgressListener
{
    public event EventHandler<string> OnChunkCompleted;
    public event EventHandler<string> OnChunkStarted;
    public event EventHandler<string> OnChunkError;

    public override void OnDone(string utteranceId)
    {
        OnChunkCompleted?.Invoke(this, utteranceId);
    }

    public override void OnError(string utteranceId)
    {
        OnChunkError?.Invoke(this, utteranceId);
    }

    public override void OnStart(string utteranceId)
    {
        OnChunkStarted?.Invoke(this, utteranceId);
    }

    public override void OnError(string utteranceId, [GeneratedEnum] TextToSpeechError error)
    {
        OnChunkError?.Invoke(this, utteranceId);
    }
}

public class TtsChunk
{
    public string Id { get; set; }
    public string Text { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}

