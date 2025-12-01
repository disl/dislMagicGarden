using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class NewStoryPage : ContentPage
{
	public NewStoryPage(NewStoryViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
    }
}