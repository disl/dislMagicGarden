namespace dislMagicGarden.Models
{
    public enum GenerationMode
    {
        TextOnly,      // Nur Text (günstig mit DeepSeek)
        FullStory      // Text + Bilder (Hybrid)
    }

    public class FairyTaleRequest
    {
        public string Theme { get; set; } = string.Empty;
        public string Style { get; set; } = "Classic";

        public FairyTaleType FairyTaleType { get; set; } = FairyTaleType.Adventure;

        public GenerationMode Mode { get; set; } = GenerationMode.TextOnly;
        public int ImageCount { get; set; } = 4;
        public int AgeGroup { get; internal set; }
        public int Duration_min { get; internal set; } = 5;

        public GenderOption Gender_male { get; set; } = GenderOption.Neutral;

    }

    public class FairyTaleResponse
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Characters { get; set; } = new();
        public string Story { get; set; } = string.Empty;
        public string Moral { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
        public List<string> ImagePrompts { get; set; } = new();
        public CostBreakdown Cost { get; set; } = new();
        public TimeSpan GenerationTime { get; set; }

        public bool HasImages => ImageUrls?.Any() == true;
    }

    public class CostBreakdown
    {
        public decimal TextCost { get; set; }
        public decimal ImageCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "USD";

        public override string ToString()
        {
            return $"{TotalCost:F4} {Currency} (Text: {TextCost:F4}, Images: {ImageCost:F4})";
        }
    }

    public class TextOnlyResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Story { get; set; } = string.Empty;
        public string Moral { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }
}

