namespace dislMagicGarden.Models
{
    public class Story
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public string ChildName { get; set; }
        public string Theme { get; set; }
        public string Mood { get; set; }   // "beruhigend", "lustig", "abenteuerlich"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Chapter> Chapters { get; set; } = new();
    }

}
