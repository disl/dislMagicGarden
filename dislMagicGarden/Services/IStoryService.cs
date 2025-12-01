using dislMagicGarden.Models;

namespace dislMagicGarden.Services
{
    public interface IStoryService
    {
        Task<Story> GenerateStoryAsync(StorySettings settings);
    }
}
