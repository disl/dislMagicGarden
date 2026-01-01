#if ANDROID
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
#endif

using CommunityToolkit.Maui.Views;
using System.Text;

namespace dislMagicGarden.Views;

public partial class VoiceRecognitionPage : Popup<string>
{
    bool isListening = false;
    bool IsListening
    {
        get { return isListening; }

        set
        {
            isListening = value;

            startButton.IsVisible = !isListening;
            stopButton.IsVisible = isListening;
        }
    }

#if ANDROID
    SpeechRecognizer? recognizer;
    Intent? voiceIntent;
    //StringBuilder textBuilder = new();

#endif

    public VoiceRecognitionPage()
    {
        InitializeComponent();
    }

    private async void OnStartListeningClicked(object sender, EventArgs e)
    {
#if ANDROID
        if (IsListening)
            return;

        if (!await CheckMicrophonePermissionAsync())
        {
            await Application.Current.MainPage.DisplayAlert(Properties.Resources.Access_denied, Properties.Resources.Microphone_access_is_required, "OK");
            return;
        }

        textBuilder.Clear();
        txtResult.Text = string.Empty;


        var context = Platform.CurrentActivity ?? Android.App.Application.Context;

        voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Properties.Resources.Speak_now);
        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
        voiceIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, Platform.AppContext.PackageName);

        // Wichtig für kontinuierliche Erkennung
        voiceIntent.PutExtra(RecognizerIntent.ExtraPartialResults, true);

        // Timeout-Einstellungen anpassen
        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 2500);
        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 5000);

        // Mehr Ergebnisse anfordern
        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 10);

        // Optional: Sprach-Erkennung verbessern
        voiceIntent.PutExtra(RecognizerIntent.ExtraPreferOffline, false);
        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak now...");

        //var listener = new ContinuousSpeechListener(OnTextRecognized, RestartListening);
        var listener = new ContinuousSpeechListener(
                            OnTextRecognized,
                            RestartListening,
                            RestartListening,
                            () => IsListening  // <--- Das ist der _statusCheck
                        );
        recognizer = SpeechRecognizer.CreateSpeechRecognizer(context);



        recognizer.SetRecognitionListener(listener);

        



        IsListening = true;
        recognizer.StartListening(voiceIntent);
#else
        if(Application.Current != null && Application.Current.MainPage != null)
            await Application.Current.MainPage.DisplayAlert("Nicht unterstützt", "Nur auf Android verfügbar.", "OK");
#endif
    }

    private async void OnStopListeningClicked(object sender, EventArgs e)
    {
#if ANDROID
        IsListening = false;
        recognizer?.StopListening();
        //recognizer?.Destroy();

        if (!string.IsNullOrEmpty(txtResult.Text))
        {
            await CloseMe(txtResult.Text);
        }
#endif
    }

    async Task CloseMe(dynamic param)
    {
        // Popup zuerst schließen
        if (Handler != null)
        {
            CloseAsync(param); // Bei Popup<TResult> -> kein await, sofort Ergebnis setzen
        }

        // Danach Navigation
        if (Navigation.ModalStack.Any())
        {
            await Navigation.PopModalAsync();
        }
    }

    private readonly StringBuilder textBuilder = new();
    private string bestSentence = "";

#if ANDROID

    private void OnTextRecognized(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        text = text.Trim();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Neuer Text ist eine Erweiterung des alten
            if (text.Length > bestSentence.Length &&
                (text.Contains(bestSentence) || bestSentence.Contains(text)))
            {
                bestSentence = text;
                txtResult.Text = bestSentence;
            }
        });
    }

    //private void OnTextRecognized(string text)
    //{
    //    if (string.IsNullOrWhiteSpace(text)) return;

    //    MainThread.BeginInvokeOnMainThread(() =>
    //    {
    //        if (!textBuilder.ToString().EndsWith(text))
    //            textBuilder.AppendLine(text);

    //        txtResult.Text = textBuilder.ToString();
    //    });
    //}

    private void RestartListening()
    {
        if (!IsListening || voiceIntent == null) return;
        recognizer?.StartListening(voiceIntent);
    }
#endif

    private async Task<bool> CheckMicrophonePermissionAsync()
    {
#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        return status == PermissionStatus.Granted;
#else
            return false;
#endif
    }


#if ANDROID
    public class ContinuousSpeechListener : Java.Lang.Object, IRecognitionListener
    {
        private readonly Action<string> _onResult;
        private readonly Action _onEnd;
        private readonly Action _onRestart;
        // Das ist die Funktion, die den Status von IsListening abfragt
        private readonly Func<bool> _statusCheck;

        public ContinuousSpeechListener(Action<string> onResult, Action onEnd,  Action onRestart, Func<bool> statusCheck)
        {
            _onResult = onResult;
            _onEnd = onEnd;
            _onRestart = onRestart;
            _statusCheck = statusCheck;
        }

        public void OnReadyForSpeech(Bundle? @params) { }
        public void OnBeginningOfSpeech() { }
        public void OnRmsChanged(float rmsdB) { }
        public void OnBufferReceived(byte[]? buffer) { }
        public void OnEndOfSpeech() => _onEnd.Invoke();

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            // bestimmte Fehler ignorieren und neu starten
            if (error == SpeechRecognizerError.NoMatch || error == SpeechRecognizerError.SpeechTimeout)
                _onEnd.Invoke();
        }

        public void OnResults(Bundle? results)
        {
            var matches = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
                _onResult.Invoke(matches[0]);

            // HIER wird _statusCheck() aufgerufen. 
            // Es schaut in Ihrer Page nach: Ist IsListening noch true?
            if (_statusCheck != null && _statusCheck.Invoke())
            {
                _onRestart?.Invoke();
            }
        }

        public void OnPartialResults(Bundle? partialResults)
        {
            var matches = partialResults?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
                _onResult.Invoke(matches[0]);
        }

        public void OnEvent(int eventType, Bundle? @params) { }
    }
#endif
}
