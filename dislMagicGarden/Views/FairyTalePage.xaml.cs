using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class FairyTalePage : FairyBasePage
{

    public FairyTalePage(FairyTaleViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }

    private void Picker_Focused(object sender, FocusEventArgs e)
    {

    }
}