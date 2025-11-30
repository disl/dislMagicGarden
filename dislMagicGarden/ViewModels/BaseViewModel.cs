using CommunityToolkit.Mvvm.ComponentModel;

namespace dislMagicGarden.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        bool isBusy;

        [ObservableProperty]
        string title;
    }
}
