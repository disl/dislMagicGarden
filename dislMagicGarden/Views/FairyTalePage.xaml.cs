using dislMagicGarden.ViewModels;
using System.Windows.Input;

namespace dislMagicGarden.Views;

public partial class FairyTalePage : FairyBasePage
{
    private int _storyCount = 0;
    public ICommand GenerateStoryCommand { get; }
    public ICommand ShowRewardAdCommand { get; }

    public FairyTalePage(FairyTaleViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;

      
    }

    private static DateTime _lastAdShown = DateTime.MinValue;

   



    private void Picker_Focused(object sender, FocusEventArgs e)
    {

    }
}