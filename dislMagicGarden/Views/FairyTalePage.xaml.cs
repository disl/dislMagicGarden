using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class FairyTalePage : FairyBasePage
{

    public FairyTalePage(FairyTaleViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}