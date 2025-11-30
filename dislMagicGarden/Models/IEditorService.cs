using SkiaSharp;

namespace dislMagicGarden.Models
{
    public interface IEditorService
    {
        Task<SKBitmap> EditImageAsync(string imagePath);
        Task<string> SaveEditedImageAsync(Chapter chapter);
    }
}
