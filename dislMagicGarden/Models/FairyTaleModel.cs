using System.Collections.ObjectModel;

namespace dislMagicGarden.Models
{
    public class FairyTaleModel
    {
        public string Title { get; set; }
        public string Story { get; set; }
        public string Moral { get; set; }

        // KI-Kosten-Info (optional bei TextOnly)
        public string Cost { get; set; }

        // KI-Bilder (falls im Premium Modus)
        public ObservableCollection<string> ImageUrls { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Characters { get; set; }

        // Convenience-Property
        public bool HasImages => ImageUrls?.Count > 0;

        public string ImagePromptsCombined { get; set; }

        public string CostText { get; set; }
        public string DurationText { get; set; }
        public List<QuizQuestion> QuizQuestions { get; internal set; }

        public FairyTaleModel() { }

        public FairyTaleModel(string title, string story, string moral, string cost = "")
        {
            Title = title;
            Story = story;
            Moral = moral;
            Cost = cost;
        }

    }
}
