using CommunityToolkit.Maui.Media;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace dislMagicGarden.Views;

public partial class VoiceRecognitionPage : Popup<string>
{
    private CancellationTokenSource recognitionCts = new CancellationTokenSource();
    private DateTime lastRecognitionTime = DateTime.MinValue;
    private string lastProcessedText = string.Empty;
    private bool isAutoRestarting = false; // Flag für automatische Neustarts

    bool isListening = false;
    bool IsListening
    {
        get { return isListening; }
        set
        {
            isListening = value;
            startButton.IsVisible = !isListening;
            stopButton.IsVisible = isListening;
            //restartButton.IsVisible = isListening;
        }
    }

    public VoiceRecognitionPage()
    {
        InitializeComponent();
    }

    private async void OnStartListeningClicked(object sender, EventArgs e)
    {
        await StartListening();
    }

    private async Task StartListening(bool isRestart = false)
    {
        if (IsListening && !isRestart)
            return;

        try
        {
            IsListening = true;
            isAutoRestarting = false;

            // Bereits vorhandenen CTS beenden
            if (recognitionCts != null && !recognitionCts.IsCancellationRequested)
            {
                recognitionCts.Cancel();
                await Task.Delay(100); // Kurze Pause
            }

            recognitionCts = new CancellationTokenSource();

            // Nur beim ersten Start oder manuellen Neustart leeren
            if (!isRestart)
            {
                txtResult.Text = string.Empty;
                lastProcessedText = string.Empty;
            }

            // Berechtigungen prüfen
            var permissionsOk = await SpeechToText.Default.RequestPermissions(recognitionCts.Token);
            if (!permissionsOk)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Zugriff verweigert",
                    "Mikrofonzugriff ist erforderlich",
                    "OK");
                IsListening = false;
                return;
            }

            // Event-Handler registrieren (nur wenn nicht bereits registriert)
            SpeechToText.Default.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted -= OnRecognitionTextCompleted;

            SpeechToText.Default.RecognitionResultUpdated += OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted += OnRecognitionTextCompleted;

            // Optionen für kontinuierliche Erkennung
            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.CurrentCulture,
                ShouldReportPartialResults = true
            };

            // Erkennung starten
            await SpeechToText.Default.StartListenAsync(options, recognitionCts.Token);

            Console.WriteLine($"### Speech recognition started (Restart: {isRestart})");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("### Recognition cancelled on start");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"### Error starting recognition: {ex.Message}");

            if (!isAutoRestarting) // Nur Fehler anzeigen wenn nicht automatischer Neustart
            {
                await Application.Current.MainPage.DisplayAlert("Fehler",
                    "Spracherkennung konnte nicht gestartet werden", "OK");
            }

            IsListening = false;
        }
    }

    private void OnRecognitionTextUpdated(object sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
    {
        lastRecognitionTime = DateTime.Now;

        if (!string.IsNullOrEmpty(e.RecognitionResult))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessNewText(e.RecognitionResult, isFinalResult: false);
            });
        }
    }

    private async void OnRecognitionTextCompleted(object sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        lastRecognitionTime = DateTime.Now;

        if (!string.IsNullOrEmpty(e.RecognitionResult.Text))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessNewText(e.RecognitionResult.Text, isFinalResult: true);
            });
        }

        // Automatisch neu starten (nur wenn nicht bereits im Neustart)
        if (IsListening && !isAutoRestarting)
        {
            await AutoRestartRecognition();
        }
    }

    private string confirmedText = ""; // Globaler Speicher für fertige Sätze


    private void ProcessNewText(string newText, bool isFinalResult)
    {
        if (string.IsNullOrWhiteSpace(newText)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (isFinalResult)
            {
                // Wir hängen den Text an, aber ohne manuellen Punkt!
                // Wir prüfen nur, ob wir ein Leerzeichen brauchen.
                if (!string.IsNullOrEmpty(confirmedText) && !confirmedText.EndsWith(" "))
                {
                    confirmedText += " ";
                }

                confirmedText += newText.Trim();
                txtResult.Text = confirmedText;
            }
            else
            {
                // Während des Sprechens: Feststehender Text + aktuelle Vorschau
                string separator = (string.IsNullOrEmpty(confirmedText) || confirmedText.EndsWith(" ")) ? "" : " ";
                txtResult.Text = confirmedText + separator + newText;
            }

            // Cursor immer ans Ende setzen
            txtResult.CursorPosition = txtResult.Text.Length;
        });
    }

    private void ReplaceOrAppendPartialText(string newPartialText)
    {
        var currentText = txtResult.Text.Trim();

        // Wenn Textbox leer ist
        if (string.IsNullOrEmpty(currentText))
        {
            txtResult.Text = newPartialText + " ";
            return;
        }

        // Finde den letzten Punkt
        var lastDotIndex = currentText.LastIndexOf('.');

        if (lastDotIndex >= 0 && lastDotIndex < currentText.Length - 1)
        {
            // Es gibt Text nach dem letzten Punkt -> ersetzen
            var baseText = currentText.Substring(0, lastDotIndex + 2); // Punkt + Leerzeichen
            txtResult.Text = baseText + newPartialText + " ";
        }
        else if (lastDotIndex == currentText.Length - 1)
        {
            // Letztes Zeichen ist ein Punkt -> anhängen
            txtResult.Text = currentText + " " + newPartialText + " ";
        }
        else
        {
            // Kein Punkt vorhanden -> gesamten Text ersetzen (da unvollständig)
            txtResult.Text = newPartialText + " ";
        }
    }

    private bool IsTextAlreadyInTextBox(string newText)
    {
        if (string.IsNullOrEmpty(newText) || string.IsNullOrEmpty(txtResult.Text))
            return false;

        var existingText = txtResult.Text;

        // Direkte Übereinstimmung
        if (existingText.Contains(newText, StringComparison.OrdinalIgnoreCase))
            return true;

        // Ähnlichkeitsprüfung für längere Texte
        if (newText.Split(' ').Length > 2)
        {
            var existingWords = existingText.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(w => w.ToLower())
                                           .Distinct()
                                           .ToList();

            var newWords = newText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(w => w.ToLower())
                                 .Distinct()
                                 .ToList();

            var commonWords = newWords.Count(w => existingWords.Contains(w));
            var similarity = (double)commonWords / newWords.Count;

            return similarity > 0.7; // Mehr als 70% der Wörter sind bereits vorhanden
        }

        return false;
    }

    private async Task AutoRestartRecognition()
    {
        if (!IsListening || recognitionCts == null || recognitionCts.IsCancellationRequested)
            return;

        isAutoRestarting = true;

        try
        {
            Console.WriteLine("### Auto-restarting recognition...");

            // Kurze Pause
            await Task.Delay(300);

            // Event-Handler temporär entfernen
            SpeechToText.Default.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted -= OnRecognitionTextCompleted;

            // Erkennung stoppen
            try
            {
                await SpeechToText.Default.StopListenAsync(CancellationToken.None);
            }
            catch
            {
                // Ignorieren wenn bereits gestoppt
            }

            await Task.Delay(200);

            // Neu starten MIT Beibehaltung des Textes
            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.CurrentCulture,
                ShouldReportPartialResults = true
            };

            // Event-Handler wieder registrieren
            SpeechToText.Default.RecognitionResultUpdated += OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted += OnRecognitionTextCompleted;

            await SpeechToText.Default.StartListenAsync(options, recognitionCts.Token);

            Console.WriteLine("### Auto-restart completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"### Auto-restart error: {ex.Message}");

            // Nach 2 Sekunden erneut versuchen
            await Task.Delay(2000);
            if (IsListening)
            {
                await AutoRestartRecognition();
            }
        }
        finally
        {
            isAutoRestarting = false;
        }
    }

    private async Task StopListening()
    {
        try
        {
            IsListening = false;
            isAutoRestarting = false;

            // Handler sofort entfernen, um keine weiteren Events zu feuern
            SpeechToText.Default.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted -= OnRecognitionTextCompleted;

            if (recognitionCts != null)
            {
                recognitionCts.Cancel();
                recognitionCts.Dispose();
                recognitionCts = null;
            }

            await SpeechToText.Default.StopListenAsync(CancellationToken.None);
            Console.WriteLine("### Stopped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"### Error stopping: {ex.Message}");
        }
    }

    // Der Button ruft nun direkt das Schließen auf
    private async void OnStopListeningClicked(object sender, EventArgs e)
    {
        await StopListening();
        string finalResult = txtResult.Text?.Trim() ?? "";

        // Nur hier am Ende einen Punkt setzen, falls gar keiner da ist
        if (!string.IsNullOrEmpty(finalResult) && !finalResult.EndsWith(".") && !finalResult.EndsWith("!") && !finalResult.EndsWith("?"))
        {
            finalResult += ".";
        }

        await CloseAsync(finalResult);
    }

    private async void OnRestartListeningClicked(object sender, EventArgs e)
    {
        // Manueller Neustart - Text bleibt erhalten
        await ManualRestart();
    }

    private async Task ManualRestart()
    {
        if (IsListening)
        {
            // Erst stoppen
            try
            {
                SpeechToText.Default.RecognitionResultUpdated -= OnRecognitionTextUpdated;
                SpeechToText.Default.RecognitionResultCompleted -= OnRecognitionTextCompleted;

                await SpeechToText.Default.StopListenAsync(CancellationToken.None);
            }
            catch { }

            await Task.Delay(300);
        }

        // Neu starten MIT Beibehaltung des Textes
        await StartListening(true);
    }

    private async Task CloseMe(dynamic param)
    {
        // Sicherstellen, dass alles gestoppt ist
        await StopListening();

        // Popup schließen
        if (Handler != null)
        {
            CloseAsync(param);
        }
    }
}

