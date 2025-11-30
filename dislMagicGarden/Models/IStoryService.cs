namespace dislMagicGarden.Models
{
    public interface IIllustrationService
    {
        Task GenerateIllustrationsAsync(Story story);
    }

    public interface IEditorService
    {
        Task<string> EditImageAsync(string imagePath);
        // liefert Pfad zum bearbeiteten Bild
    }

    public interface IBookExportService
    {
        Task<string> ExportToPdfAsync(Story story, string outputPath);
    }

}
