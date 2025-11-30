using dislMagicGarden.Models;

namespace dislMagicGarden.Services
{
    public class StoryService : IStoryService
    {
        public async Task<string> GenerateStory(string name, string place, string animal)
        {
            string prompt = $"Erstelle eine beruhigende Kinder-Gute-Nacht-Geschichte " +
                            $"über {name} und sein Tierfreund {animal} im Setting {place}. " +
                            $"Nutze einfache Sprache, klare Bilder und 4-6 kurze Abschnitte.";

            string storyText = await _aiClient.GenerateTextAsync(prompt);
            return storyText;
        }

        public Task<Story> GenerateStoryAsync(StorySettings settings)
        {
            throw new NotImplementedException();
        }
    }

}
