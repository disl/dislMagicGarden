using dislMagicGarden.Services;
using dislMagicGarden.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace dislMagicGarden.Views;

public partial class FairyTalePage : FairyBasePage
{
    private int _storyCount = 0;
    public ICommand GenerateStoryCommand { get; }
    public ICommand ShowRewardAdCommand { get; }

    private readonly AdService _adService;

    public FairyTalePage(FairyTaleViewModel vm, AdService? adService)
    {
        InitializeComponent();

        BindingContext = vm;

        _adService = adService;
        _adService.OnAdStatusChanged += OnAdStatusChanged;

        InitializeAds();
    }

    protected async override void OnAppearing()
    {        
        // Bei Seitenwechsel Ad prüfen
        //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //{
        //    _adService.TryShowInterstitial();
        //    return false; // Nur einmal ausführen
        //});
    }

    private void InitializeAds()
    {
        try
        {
            // Banner Ad hinzufügen
            //AddBannerAd();

            // Interstitial Ad im Hintergrund laden
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000); // 3 Sekunden warten
                await _adService.LoadRewardedAsync(); //  LoadInterstitialAsync();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HomePage Ad Initialization Error: {ex.Message}");
        }
    }

    private static DateTime _lastAdShown = DateTime.MinValue;


    private void OnAdStatusChanged(object sender, string status)
    {
        Debug.WriteLine($"Ad Status: {status}");
        // Optional: UI aktualisieren
    }


    private void Picker_Focused(object sender, FocusEventArgs e)
    {

    }

    private async void QuickGenerateTextOnly_Clicked(object sender, EventArgs e)
    {
        _storyCount++;

        // 2. Nach jeder 2. Story Ad prüfen
        if (_storyCount % 2 == 0)
        {
            // Kleine Pause nach Erfolg
            await Task.Delay(500);

            // Ad versuchen zu zeigen
            bool adShown = await _adService.TryShowInterstitial();

            if (!adShown)
            {
                Debug.WriteLine("Keine Ad verfügbar, fahre fort...");
            }
        }

        await ((FairyTaleViewModel)BindingContext).QuickGenerateTextOnlyCommand.ExecuteAsync(null);
    }

}