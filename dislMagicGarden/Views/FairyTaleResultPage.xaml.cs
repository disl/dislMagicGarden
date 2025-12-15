using dislMagicGarden.Models;
using dislMagicGarden.Services;
using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class FairyTaleResultPage : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FairyTaleResultViewModel vm)
            await vm.LoadVoicesAsync();
    }

    public FairyTaleResultPage(FairyTaleModel fairyTale, ITextToSpeechService textToSpeechService)
    {
        InitializeComponent();
        BindingContext = new FairyTaleResultViewModel(fairyTale, Close, textToSpeechService);
    }

    private async void Close()
    {
        await Navigation.PopModalAsync(true);
    }
}