namespace dislMagicGarden.Models
{
    public interface IIllustrationService
    {
        Task GenerateIllustrationsAsync(Story story);
    }

    public interface IBookExportService
    {
        Task<string> ExportToPdfAsync(Story story, string outputPath);
    }

}
