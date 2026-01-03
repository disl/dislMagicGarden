using dislMagicGarden.Services;

namespace dislMagicGarden.Views;

public partial class ColoringGenerator : FairyBasePage
{
    string Topic { get; set; }
    string Title { get; set; }

    string apiPrompt_coloring_page = "";
    private string? currentImageUrl;
    private readonly ImageGeneratorService _generatorService = new();
    private readonly PdfExportService _pdfService = new();

    public ColoringGenerator(string prompt, string title)
    {
        InitializeComponent();

        Topic = prompt;
        Title= title;

        apiPrompt_coloring_page = "Coloring page for kids. " +
                              "Start of topic: " + Environment.NewLine +
                              $"{Topic} " +
                              " End of topic" + Environment.NewLine +
                              " thick black outlines, white background, no shading, high contrast, line art.";
    }

    private async void OnGenerate_ColoringPageClicked(object sender, EventArgs e)
    {
        bool flowControl = await GeneratePage(true);
        if (!flowControl)
        {
            return;
        }
    }
    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        bool flowControl = await GeneratePage(false);
        if (!flowControl)
        {
            return;
        }
    }
    private async Task<bool> GeneratePage(bool IsColoringPage)
    {
        string theme = IsColoringPage ? apiPrompt_coloring_page : Topic;
        if (string.IsNullOrWhiteSpace(theme)) return false;

        // UI-Status setzen
        GenerateBtn.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        ImageFrame.IsVisible = false;

        currentImageUrl = await _generatorService.GenerateColoringPage(theme);

        if (!string.IsNullOrEmpty(currentImageUrl))
        {
            ResultImage.Source = ImageSource.FromUri(new Uri(currentImageUrl));
            ImageFrame.IsVisible = true;
            SaveBtn.IsVisible = true;
        }
        else
        {
            await DisplayAlert(Properties.Resources.Error, Properties.Resources.Image_could_not_be_generated, "OK");
        }

        LoadingIndicator.IsRunning = false;
        GenerateBtn.IsEnabled = true;
        return true;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(currentImageUrl)) return;

        var saveService = new ImageSaveService();
        // Name generieren, z.B. "Ausmalbild_Löwe.png"
        string fileName = $"Coloring_page_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        await saveService.SaveImageToGallery(currentImageUrl, fileName);
    }
    private async void OnCreatePDFBtnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(currentImageUrl)) return;

        try
        {
            LoadingIndicator.IsRunning = true;

            // currentImageUrl ist die URL von der API
            // PromptEntry.Text ist der Text des Users
            await _pdfService.CreateAndShowPdf(Title, Topic, currentImageUrl);
        }
        catch (Exception ex)
        {
            await DisplayAlert(Properties.Resources.Error, 
                Properties.Resources.PDF_could_not_be_created + ": " + ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
        }
    }
}