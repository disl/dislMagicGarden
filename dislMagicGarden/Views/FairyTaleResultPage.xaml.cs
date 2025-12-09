using dislMagicGarden.Models;
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

    public FairyTaleResultPage(FairyTaleModel fairyTale)
    {
        InitializeComponent();
        BindingContext = new FairyTaleResultViewModel(fairyTale, Close);
    }

    private async void Close()
    {
        await Navigation.PopModalAsync(true);
    }
}