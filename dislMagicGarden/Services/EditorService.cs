using dislMagicGarden.Models;
using SkiaSharp;

namespace dislMagicGarden.Services
{
    public class EditorService : IEditorService
    {
        private readonly string _outputFolder;

        public EditorService()
        {
            _outputFolder = FileSystem.AppDataDirectory;

            // Ordner für bearbeitete Bilder sicherstellen
            var editsDir = Path.Combine(_outputFolder, "edited");
            if (!Directory.Exists(editsDir))
                Directory.CreateDirectory(editsDir);
        }

        public async Task<SKBitmap> EditImageAsync(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentNullException(nameof(imagePath));

            using var stream = File.OpenRead(imagePath);
            return SKBitmap.Decode(stream);
        }


        public async Task<SKBitmap> LoadBaseImageAsync(string imagePath)
        {
            using var stream = File.OpenRead(imagePath);
            return SKBitmap.Decode(stream);
        }

        /// <summary>
        /// Speichert das bearbeitete Bild aus dem Chapter als PNG.
        /// </summary>
        public async Task<string> SaveEditedImageAsync(Chapter chapter)
        {
            string editedDir = Path.Combine(_outputFolder, "edited");

            string outputFile = Path.Combine(
                editedDir,
                $"chapter_{chapter.Number}_{DateTime.Now:yyyyMMddHHmmss}.png");

            // STEP 1: Basisbild laden
            var baseBitmap = await LoadBaseImageAsync(chapter.EffectiveImagePath);

            // STEP 2: Ein Canvas erzeugen, um Base + Zeichnungen zu rendern
            using var surface = SKSurface.Create(new SKImageInfo(baseBitmap.Width, baseBitmap.Height));
            var canvas = surface.Canvas;

            // Weißer Hintergrund (oder transparent)
            canvas.Clear(SKColors.Transparent);

            // Schritt 3: BaseImage zeichnen
            canvas.DrawBitmap(baseBitmap, 0, 0);

            // Schritt 4: Kinder-Zeichnungen holen
            if (chapter.EditorStrokes != null)
            {
                foreach (var stroke in chapter.EditorStrokes)
                {
                    using var paint = new SKPaint
                    {
                        Color = stroke.Color,
                        StrokeWidth = stroke.StrokeWidth,
                        IsAntialias = true,
                        StrokeCap = SKStrokeCap.Round
                    };

                    for (int i = 1; i < stroke.Points.Count; i++)
                    {
                        var p1 = stroke.Points[i - 1];
                        var p2 = stroke.Points[i];
                        canvas.DrawLine(p1, p2, paint);
                    }
                }
            }

            // Schritt 5: Ergebnis speichern
            using var img = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);

            using var outStream = File.OpenWrite(outputFile);
            data.SaveTo(outStream);

            return outputFile;
        }
    }
}