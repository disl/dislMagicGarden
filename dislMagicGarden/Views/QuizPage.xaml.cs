using dislMagicGarden.Models;
using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

[QueryProperty(nameof(Questions), "Questions")]
public partial class QuizPage : FairyBasePage 
{
    private List<QuizQuestion> _questions;

    private Random _random = new Random();

    // Für SimpleAudioPlayer
    //private IAudioPlayer _fireworksSound;
    //private IAudioPlayer _wowSound;

    public List<QuizQuestion> Questions
    {
        get => _questions;
        set
        {
            _questions = value;

            if (BindingContext is QuizViewModel vm)
            {
                vm.LoadQuestions(value);
            }

            OnPropertyChanged();
            // Hier kannst du die Fragen weiterverarbeiten
        }
    }



    public QuizPage(QuizViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;

        // Event abonnieren
        vm.FireworksRequested += OnFireworksRequested;
    }

    private async void OnFireworksRequested(object sender, EventArgs e)
    {
        await ShowFireworks();
    }

    private async Task ShowFireworks()
    {
        // Overlay sichtbar machen
        FireworksOverlay.IsVisible = true;
        FireworksOverlay.InputTransparent = true; // Klicks werden durchgereicht

        // 10 "Raketen" starten
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(LaunchFirework());
            await Task.Delay(200); // Kurze Pause zwischen Raketen
        }

        await Task.WhenAll(tasks);

        // Overlay ausblenden und bereinigen
        FireworksOverlay.Children.Clear();
        FireworksOverlay.IsVisible = false;
    }

    private async Task LaunchFirework()
    {
        // Zufällige Position
        double startX = _random.Next(20, (int)(this.Width - 100));
        double startY = this.Height - 50; // Vom unteren Rand

        // Raketen-Label erstellen
        var rocket = new Label
        {
            Text = "🚀",
            FontSize = 30,
            TranslationX = startX,
            TranslationY = startY,
            Opacity = 1,
            Rotation = 0
        };

        // Zum Overlay hinzufügen
        FireworksOverlay.Children.Add(rocket);

        // Rakete fliegt nach oben
        await rocket.TranslateTo(startX, 100, 800, Easing.CubicOut);

        // Rakete verschwindet
        FireworksOverlay.Children.Remove(rocket);

        // EXPLOSION! Mehrere Partikel erstellen
        await CreateExplosion(startX, 100);
    }

    private async Task CreateExplosion(double x, double y)
    {
        var explosionTasks = new List<Task>();
        int particleCount = _random.Next(8, 15);

        for (int i = 0; i < particleCount; i++)
        {
            // Zufälliges Partikel-Symbol
            string symbol = _random.Next(3) switch
            {
                0 => "✨",
                1 => "🎆",
                _ => "🎇"
            };

            var particle = new Label
            {
                Text = symbol,
                FontSize = _random.Next(20, 40),
                TranslationX = x,
                TranslationY = y,
                Opacity = 1,
                Rotation = _random.Next(0, 360)
            };

            FireworksOverlay.Children.Add(particle);

            // Zufällige Richtung
            double targetX = x + _random.Next(-150, 150);
            double targetY = y + _random.Next(-150, 150);

            // Partikel fliegt und verblasst
            var translateTask = particle.TranslateTo(targetX, targetY, 800, Easing.CubicOut);
            var fadeTask = particle.FadeTo(0, 800);

            explosionTasks.Add(Task.WhenAll(translateTask, fadeTask)
                .ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        FireworksOverlay.Children.Remove(particle);
                    });
                }));
        }

        await Task.WhenAll(explosionTasks);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is QuizViewModel vm)
        {
            vm.FireworksRequested -= OnFireworksRequested;
        }
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(true);
    }

}