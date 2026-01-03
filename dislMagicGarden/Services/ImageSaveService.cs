using CommunityToolkit.Maui.Storage;

namespace dislMagicGarden.Services;

public class ImageSaveService
{
    public async Task SaveImageToGallery(string imageUrl, string fileName)
    {
        using var httpClient = new HttpClient();

        // 1. Bild von API-URL streamen
        var imageStream = await httpClient.GetStreamAsync(imageUrl);

        // 2. FileSaver nutzen (öffnet den nativen Speicherdialog)
        var fileSaveResult = await FileSaver.Default.SaveAsync(fileName, imageStream, CancellationToken.None);

        if (fileSaveResult.IsSuccessful)
        {
            await Application.Current.MainPage.DisplayAlert("Ok", Properties.Resources.Image_saved_to_gallery, "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert(Properties.Resources.Error, Properties.Resources.Saving_cancelled, "OK");
        }
    }
}

