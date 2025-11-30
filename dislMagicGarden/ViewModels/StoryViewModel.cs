using dislMagicGarden.Models;

namespace dislMagicGarden.ViewModels
{
    public class StoryViewModel : BaseViewModel
    {
        private readonly IStoryService _storyService;

        public string ChildName { get; set; }
        public string Setting { get; set; }
        public string HeroAnimal { get; set; }

        private string _generatedStory;
        public string GeneratedStory
        {
            get => _generatedStory;
            set => SetProperty(ref _generatedStory, value);
        }

        public Command GenerateStoryCommand { get; }

        public StoryViewModel(IStoryService storyService)
        {
            _storyService = storyService;
            GenerateStoryCommand = new Command(async () => await GenerateStory());
        }

        private async Task GenerateStory()
        {
            GeneratedStory = await _storyService.GenerateStory(
                ChildName, Setting, HeroAnimal
            );
        }
    }

}
