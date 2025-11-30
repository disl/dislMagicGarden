using SkiaSharp;

namespace dislMagicGarden.Models
{
    public interface IEditorService
    {
        Task<string> SaveEditedImageAsync(Chapter chapter);
        Task<SKBitmap> LoadBaseImageAsync(string imagePath);
    }
}
