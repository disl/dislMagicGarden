using CommunityToolkit.Maui.Layouts;
using dislMagicGarden.Models;
using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class FairyTalePage : FairyBasePage
{
    private readonly IRewardedAdService _adService;

    public FairyTalePage(FairyTaleViewModel vm, IRewardedAdService adService)
    {
        InitializeComponent();

        BindingContext = vm;

        _adService = adService;
        _adService.LoadAd(); // Frühzeitig laden!
    }

    private static DateTime _lastAdShown = DateTime.MinValue;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Zeige den Dialog nur, wenn seit dem letzten Mal 2 Stunden vergangen sind
        if ((DateTime.Now - _lastAdShown).TotalHours < 2)
            return;

        // Kurze Verzögerung, damit die Seite erst fertig geladen wird
        await Task.Delay(800);

        // Prüfen, ob eine Anzeige bereit ist
        if (_adService.IsLoaded)
        {
            bool answer = await DisplayAlert(
                Properties.Resources.Support_the_project,
                Properties.Resources.Would_you_like_to_watch_a_short_video,
                Properties.Resources.Watch_video, // Ja-Button
                Properties.Resources.Maybe_later // Nein-Button
            );

            if (answer)
            {
                _adService.ShowAd((amount) =>
                {
                    // Belohnung geben
                    DisplayAlert(Properties.Resources.Thank_you, Properties.Resources.You_have_received_n_points.Replace("%1", amount.ToString()), "OK");
                });
            }
        }
    }

    private void Picker_Focused(object sender, FocusEventArgs e)
    {

    }
}