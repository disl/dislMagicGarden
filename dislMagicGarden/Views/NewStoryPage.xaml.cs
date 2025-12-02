using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class NewStoryPage : FairyBasePage
{
	public NewStoryPage(NewStoryViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
    }
}