using CommunityToolkit.Mvvm.Input;

namespace dislMagicGarden.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = "dislMagicGarden";
        }

        [RelayCommand]
        async Task GoToNewStory()
        {
            await Shell.Current.GoToAsync("NewStoryPage");
        }
    }
}
