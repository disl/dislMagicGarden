using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System.Text;

namespace dislMagicGarden.Services
{
    public class PdfExportService
    {
        public async Task CreateAndShowPdf(string title, string storyText, string imageUrl)
        {
            try
            {
                var document = new PdfDocument();
                var titleFont = new XFont("OpenSans-Regular", 22, XFontStyleEx.Bold);
                var textFont = new XFont("OpenSans-Regular", 13, XFontStyleEx.Regular);
                double margin = 50;

                // Erste Seite
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Titel zeichnen
                gfx.DrawString(title, titleFont, XBrushes.DarkSlateBlue,
                    new XRect(0, 40, page.Width, 50), XStringFormats.TopCenter);

                double currentY = 100;

                // Bild laden (optional)
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    using var httpClient = new HttpClient();
                    try
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                        string tempPath = Path.Combine(FileSystem.CacheDirectory, "temp_image.png");
                        File.WriteAllBytes(tempPath, imageBytes);
                        using (var image = XImage.FromFile(tempPath))
                        {
                            double targetWidth = page.Width - (margin * 2);
                            double targetHeight = (image.PixelHeight * targetWidth) / image.PixelWidth;

                            gfx.DrawImage(image, margin, currentY, targetWidth, targetHeight);
                            currentY += targetHeight + 30;
                        }
                        File.Delete(tempPath);
                    }
                    catch { /* Bild ignorieren */ }
                }

                // Text auf einfache Weise hinzufügen
                AddTextWithAutoPageBreak(document, ref page, ref gfx, storyText, textFont, margin, currentY);

                // PDF speichern und öffnen
                string pdfPath = Path.Combine(FileSystem.AppDataDirectory, $"Story_{Guid.NewGuid():N}.pdf");
                document.Save(pdfPath);

                if (File.Exists(pdfPath))
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(pdfPath) });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fehler", $"Das PDF konnte nicht erstellt werden: {ex.Message}", "OK");
            }
        }

        private void AddTextWithAutoPageBreak(PdfDocument document, ref PdfPage page, ref XGraphics gfx,
                                             string text, XFont font, double margin, double startY)
        {
            double currentY = startY;
            double pageWidth = page.Width;
            double pageHeight = page.Height;
            double textWidth = pageWidth - (margin * 2);

            // Text in Zeilen aufteilen
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            double lineHeight = font.GetHeight() * 1.5; // Mehr Zeilenabstand

            foreach (var line in lines)
            {
                // Wenn nötig, neue Seite erstellen
                if (currentY + lineHeight > pageHeight - margin)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    currentY = margin;
                }

                // Wenn die Zeile zu lang ist, in mehrere Zeilen aufteilen
                string remainingLine = line;
                while (!string.IsNullOrEmpty(remainingLine))
                {
                    // Finden, wie viele Wörter in diese Zeile passen
                    string lineToDraw = GetLineThatFits(remainingLine, font, textWidth, gfx, out remainingLine);

                    // Zeile zeichnen
                    gfx.DrawString(lineToDraw, font, XBrushes.Black, margin, currentY);
                    currentY += lineHeight;

                    // Neue Seite prüfen
                    if (currentY + lineHeight > pageHeight - margin && !string.IsNullOrEmpty(remainingLine))
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        currentY = margin;
                    }
                }

                // Leere Zeilen bekommen etwas weniger Abstand
                if (string.IsNullOrWhiteSpace(line))
                {
                    currentY += lineHeight / 3;
                }
            }
        }

        private string GetLineThatFits(string text, XFont font, double maxWidth, XGraphics gfx, out string remaining)
        {
            if (gfx.MeasureString(text, font).Width <= maxWidth)
            {
                remaining = "";
                return text;
            }

            // Text in Wörter teilen
            string[] words = text.Split(' ');
            StringBuilder line = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                string testLine = line.Length > 0 ? line.ToString() + " " + words[i] : words[i];

                if (gfx.MeasureString(testLine, font).Width <= maxWidth)
                {
                    line.Append(line.Length > 0 ? " " + words[i] : words[i]);
                }
                else
                {
                    // Dieses Wort passt nicht mehr
                    remaining = string.Join(" ", words, i, words.Length - i);
                    return line.ToString();
                }
            }

            remaining = "";
            return line.ToString();
        }
    }

}
