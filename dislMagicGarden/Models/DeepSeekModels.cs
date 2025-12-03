namespace dislMagicGarden.Models
{
    // Models/FairyTaleModels.cs
    public enum GenerationMode
    {
        TextOnly,      // Nur Text (günstig)
        FullStory      // Text + 4 Bilder (Premium)
    }

    public class FairyTaleRequest
    {
        public string Theme { get; set; } = string.Empty;
        public string Style { get; set; } = "Classic";
        public int AgeGroup { get; set; } = 6; // 3-10
        public GenerationMode Mode { get; set; } = GenerationMode.TextOnly;
        public int ImageCount { get; set; } = 4; // Für FullStory
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
}
