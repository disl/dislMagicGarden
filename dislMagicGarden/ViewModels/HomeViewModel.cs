using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Views;

namespace dislMagicGarden.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = "Magic Garden";
        }

        [RelayCommand]
        async Task GoToNewStory()
        {
            await Shell.Current.GoToAsync("//FairyTalePage");
        }
    }
}
