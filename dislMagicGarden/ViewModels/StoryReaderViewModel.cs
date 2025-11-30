using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;

namespace dislMagicGarden.ViewModels
{
    [QueryProperty(nameof(Story), "Story")]
    public partial class StoryReaderViewModel : BaseViewModel
    {
        [ObservableProperty]
        Story story;

        public StoryReaderViewModel()
        {
            Title = "Geschichte";
        }

        [RelayCommand]
        async Task EditChapter(Models.Chapter chapter)
        {
            await Shell.Current.GoToAsync("ChapterEditorPage",
                new Dictionary<string, object> { { "Chapter", chapter } });
        }
    }
}
