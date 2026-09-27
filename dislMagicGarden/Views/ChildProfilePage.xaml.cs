using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class ChildProfilePage : FairyBasePage
{
    public ChildProfilePage(ChildProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
