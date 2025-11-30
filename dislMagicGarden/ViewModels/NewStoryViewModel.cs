using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;

namespace dislMagicGarden.ViewModels
{
    public class NewStoryViewModel : BaseViewModel
    {
        public partial class NewStoryViewModel : BaseViewModel
        {
            private readonly IStoryService _storyService;

            public NewStoryViewModel(IStoryService storyService)
            {
                _storyService = storyService;
                Title = "Neue Geschichte";
            }

            [ObservableProperty]
            string childName;

            [ObservableProperty]
            string sidekickAnimal;

            [ObservableProperty]
            string worldSetting;

            [ObservableProperty]
            string mood = "beruhigend";

            [RelayCommand]
            async Task GenerateStory()
            {
                try
                {
                    IsBusy = true;

                    var story = await _storyService.GenerateStoryAsync(
                        childName, sidekickAnimal, worldSetting, mood);

                    await Shell.Current.GoToAsync("StoryReaderPage",
                        new Dictionary<string, object> { { "Story", story } });
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

}
