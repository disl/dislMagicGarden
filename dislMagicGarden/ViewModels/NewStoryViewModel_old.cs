using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using dislMagicGarden.Properties;
using dislMagicGarden.Services;

namespace dislMagicGarden.ViewModels
{
    public partial class NewStoryViewModel : BaseViewModel
    {
        private readonly IStoryService _storyService;
        private readonly ILanguageService _language;

        public List<string> Moods { get; }

        public NewStoryViewModel(IStoryService storyService, ILanguageService language)
        {
            _storyService = storyService;
            Title = Properties.Resources.Home_NewStory;
            _language=language;

            Moods = new()
            {
                Resources.Reassuring,
                Resources.Adventurous,
                Resources.Funny
            };
        }

        [ObservableProperty]
        string languageIso = "en";

        [ObservableProperty]
        string childName;

        [ObservableProperty]
        string sidekickAnimal;

        [ObservableProperty]
        string worldSetting;

        [ObservableProperty]
        string mood = Properties.Resources.Reassuring;


        //void OnLanguageIsoChanged(string value)
        //{
        //    _language.CurrentIso = value;

        //    ChildName = string.Empty;
        //}

        [RelayCommand]
        async Task GenerateStory()
        {
            try
            {
                IsBusy = true;

                var settings = new StorySettings
                {
                    ChildName = childName,
                    SidekickAnimal = sidekickAnimal,
                    WorldSetting = worldSetting,
                    Mood = mood
                };

                var story = await _storyService.GenerateStoryAsync(settings);

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

