using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class HomePage : ContentPage
{
	public HomePage(HomeViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}