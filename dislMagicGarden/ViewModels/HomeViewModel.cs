using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace dislMagicGarden.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [ObservableProperty] string appVersion = $"Version {AppInfo.Current.VersionString}";

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
