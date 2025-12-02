using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class HomePage : FairyBasePage
{
	public HomePage(HomeViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}