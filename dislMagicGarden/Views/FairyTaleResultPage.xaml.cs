using CommunityToolkit.Mvvm.Messaging;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using dislMagicGarden.ViewModels;
using static dislMagicGarden.ViewModels.FairyTaleResultViewModel;

namespace dislMagicGarden.Views;

public partial class FairyTaleResultPage : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FairyTaleResultViewModel vm)
            await vm.LoadVoicesAsync();
    }

    public FairyTaleResultPage(FairyTaleModel fairyTale, ITextToSpeechService textToSpeechService, SoundEffectService soundEffectService)
    {
        InitializeComponent();

        BindingContext = new FairyTaleResultViewModel(fairyTale, Close, textToSpeechService, soundEffectService);

        WeakReferenceMessenger.Default.Register<ScrollToSentenceMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (BindingContext is FairyTaleResultViewModel vm && vm.StoryChunks != null)
                {
                    // 1. Berechne den Fortschritt (0.0 bis 1.0)
                    double progress = (double)m.Index / vm.StoryChunks.Length;

                    // 2. Bestimme die maximale Scroll-Distanz
                    // ContentSize ist die gesamte Höhe des Inhalts, Height ist die sichtbare Höhe
                    double maxScrollY = StoryScrollView.ContentSize.Height - StoryScrollView.Height;

                    if (maxScrollY > 0)
                    {
                        // 3. Scrolle zum berechneten Punkt
                        // Wir nehmen progress * maxScrollY, um zum Ende zu kommen
                        await StoryScrollView.ScrollToAsync(0, maxScrollY * progress, true);
                    }
                }
            });
        });

       
    }

    private int GetTotalChunksCount()
    {
        if (BindingContext is FairyTaleResultViewModel vm && vm.StoryChunks != null)
            return vm.StoryChunks.Length;
        return 1;
    }

    private async void Close()
    {
        await Navigation.PopModalAsync(true);
    }
}