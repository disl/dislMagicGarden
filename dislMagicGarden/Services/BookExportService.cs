using dislMagicGarden.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace dislMagicGarden.Services
{

    public interface IBookExportService
    {
        Task<string> ExportStoryToPdfAsync(Story story);
    }


    public class BookExportService : IBookExportService
    {
        private readonly string _outputPath;

        public BookExportService()
        {
            _outputPath = Path.Combine(FileSystem.AppDataDirectory, "exports");
            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
        }

        public async Task<string> ExportStoryToPdfAsync(Story story)
        {
            var doc = new PdfDocument();
            doc.Info.Title = story.Title;

            var fontTitle = new XFont("Arial", 24, XFontStyle.Bold);
            var fontChapter = new XFont("Arial", 18, XFontStyle.Bold);
            var fontText = new XFont("Arial", 14, XFontStyle.Regular);

            // ---------------------
            // 1. TITELSEITE
            // ---------------------
            var titlePage = doc.AddPage();
            var gfxTitle = XGraphics.FromPdfPage(titlePage);

            gfxTitle.DrawString(story.Title,
                fontTitle,
                XBrushes.DeepPink,
                new XRect(0, 200, titlePage.Width, 50),
                XStringFormats.Center);

            gfxTitle.DrawString(Properties.Resources.A_magical_adventure_with.Replace("%1", story.ChildName),
                fontText,
                XBrushes.DarkViolet,
                new XRect(0, 260, titlePage.Width, 40),
                XStringFormats.Center);

            // ---------------------
            // 2. Kapitel-Seiten
            // ---------------------
            foreach (var ch in story.Chapters)
            {
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Kapitel Titel
                gfx.DrawString($"{Properties.Resources.Chapter} {ch.Number}",
                    fontChapter,
                    XBrushes.HotPink,
                    new XRect(40, 40, page.Width - 80, 30),
                    XStringFormats.TopLeft);

                // Textbereich
                var layoutText = new XRect(40, 100, page.Width - 80, page.Height - 200);
                gfx.DrawString(
                    ch.Text,
                    fontText,
                    XBrushes.Black,
                    layoutText,
                    XStringFormats.TopLeft
                );

                // Illustration – falls vorhanden
                if (!string.IsNullOrWhiteSpace(ch.EffectiveImagePath) && File.Exists(ch.EffectiveImagePath))
                {
                    try
                    {
                        using var imageStream = File.OpenRead(ch.EffectiveImagePath);
                        var img = XImage.FromStream(() => imageStream);

                        double imgWidth = page.Width * 0.6;
                        double ratio = imgWidth / img.PixelWidth;
                        double imgHeight = img.PixelHeight * ratio;

                        gfx.DrawImage(img,
                            (page.Width - imgWidth) / 2,
                            page.Height - imgHeight - 60,
                            imgWidth,
                            imgHeight);
                    }
                    catch
                    {
                        // Fehler ignorieren -> Bild wird übersprungen
                    }
                }
            }

            // ---------------------
            // 3. Speichern
            // ---------------------
            string filePath = Path.Combine(
                _outputPath,
                $"Story_{story.Title}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            );

            using (var fs = File.OpenWrite(filePath))
            {
                doc.Save(fs);
            }

            return filePath;
        }
    }
}