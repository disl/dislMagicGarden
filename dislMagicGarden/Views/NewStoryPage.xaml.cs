using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class NewStoryPage : FairyBasePage
{
	public NewStoryPage(FairyTaleViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
    }
}