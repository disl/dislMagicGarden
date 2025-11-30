namespace dislMagicGarden.Models
{
    public class Chapter
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public string ImageOriginalPath { get; set; }
        public string ImageEditedPath { get; set; }

        public string EffectiveImagePath =>
            string.IsNullOrWhiteSpace(ImageEditedPath) ? ImageOriginalPath : ImageEditedPath;

        // Kinderstriche
        public List<EditorStroke> EditorStrokes { get; set; } = new();
    }


}
