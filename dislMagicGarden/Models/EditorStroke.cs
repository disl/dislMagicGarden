using SkiaSharp;

namespace dislMagicGarden.Models
{
    public class EditorStroke
    {
        public SKColor Color { get; set; }
        public float StrokeWidth { get; set; } = 6f;
        public List<SKPoint> Points { get; set; } = new();
    }
}
