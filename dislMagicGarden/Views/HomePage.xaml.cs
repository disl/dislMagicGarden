#if ANDROID
using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using dislMagicGarden.Services;
using dislMagicGarden.ViewModels;
using Plugin.MauiMTAdmob;
using System.Diagnostics;

namespace dislMagicGarden.Views;

public partial class HomePage : FairyBasePage
{
    static bool m_need_for_update = true;
    private static bool _isAdLoading;
    private int _storyCount;
    private readonly AdService _adService;

    public HomePage(HomeViewModel vm, AdService adService)
    {
        InitializeComponent();

      

        _adService = adService;
        _adService.OnAdStatusChanged += OnAdStatusChanged;

        BindingContext = vm;

        //InitializeAds();
    }



    //private void InitializeAds()
    //{
    //    try
    //    {
    //        // Banner Ad hinzufügen
    //        //AddBannerAd();

    //        // Interstitial Ad im Hintergrund laden
    //        _ = Task.Run(async () =>
    //        {
    //            await Task.Delay(3000); // 3 Sekunden warten
    //            await _adService.LoadRewardedAsync();  // LoadInterstitialAsync();
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"HomePage Ad Initialization Error: {ex.Message}");
    //    }
    //}

    //private void AddBannerAd()
    //{
    //    try
    //    {
    //        var banner = _adService.CreateBannerAd();
    //        if (banner != null && MainLayout != null)
    //        {
    //            // Banner am unteren Rand hinzufügen
    //            MainLayout.Children.Add(banner);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Banner Ad Error: {ex.Message}");
    //    }
    //}



    protected async override void OnAppearing()
    {
        if (m_need_for_update)
        {

            if (IsPlayCoreApiAvailable())
            {
                await CheckForUpdates();
            }

            m_need_for_update = false;
        }

        // Bei Seitenwechsel Ad prüfen
        //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //{
        //    _adService.TryShowInterstitial();
        //    return false; // Nur einmal ausführen
        //});
    }



    bool IsPlayCoreApiAvailable()
    {
        try
        {
            var context = Android.App.Application.Context;
            var packageManager = context.PackageManager;
            var playStorePackageName = "com.android.vending";
            var intent = packageManager.GetLaunchIntentForPackage(playStorePackageName);
            return intent != null;
        }
        catch
        {
            return false;
        }
    }


    private async Task CheckForUpdates()
    {
        try
        {
            var updater = new Platforms.Android.InAppUpdater();
            await updater.CheckForUpdatesAsync();
        }
        catch (Exception ex)
        {
            //SentrySdk.CaptureException(ex);
            //await Shell.Current.DisplayAlert("Update Error", ex.Message, "OK");
        }
    }





    private static async Task<bool> ShowAdSimple()
    {
        if (_isAdLoading) return false;
        _isAdLoading = true;

        Debug.WriteLine("<<<<< DEBUG_AD: Ad Load gestartet. >>>>>");

        try
        {
            // Load rewarded ad
            bool isLoaded = CrossMauiMTAdmob.Current.IsInterstitialLoaded();
            if (isLoaded)
            {
                CrossMauiMTAdmob.Current.ShowInterstitial();

                //// Show rewarded ad
                //var result = await CrossMauiMTAdmob.Current.ShowRewarded();

                //if (result)
                //{
                //    // User earned reward
                //    Debug.WriteLine("Reward earned!");
                //}
                //else
                //{
                //    Debug.WriteLine("Ad dismissed without reward");
                //}

                return true;
            }          
        }
        finally
        {
            _isAdLoading = false;
        }
        return false;
    }

    // Minimale Callback-Klasse
    //public class DirectCallback : RewardedAdLoadCallback
    //{
    //    private TaskCompletionSource<bool> _tcs;

    //    public DirectCallback(TaskCompletionSource<bool> tcs)
    //    {
    //        _tcs = tcs;
    //    }

    //    public virtual void OnAdLoaded(RewardedAd rewardedAd)
    //    {
    //        Debug.WriteLine("DEBUG_AD: Ad geladen!");
    //        rewardedAd.FullScreenContentCallback = new DirectFullScreenCallback(_tcs);
    //        rewardedAd.Show(Platform.CurrentActivity, new DirectRewardListener(_tcs));
    //    }

    //    public override void OnAdFailedToLoad(LoadAdError error)
    //    {
    //        Debug.WriteLine($"DEBUG_AD: Fehler beim Laden: {error.Message}");
    //        _tcs.TrySetResult(false);
    //    }
    //}

    //public class DirectFullScreenCallback : FullScreenContentCallback
    //{
    //    private TaskCompletionSource<bool> _tcs;

    //    public DirectFullScreenCallback(TaskCompletionSource<bool> tcs) => _tcs = tcs;

    //    public override void OnAdDismissedFullScreenContent() => _tcs.TrySetResult(false);
    //}

    //public class DirectRewardListener : Java.Lang.Object, IOnUserEarnedRewardListener
    //{
    //    private TaskCompletionSource<bool> _tcs;

    //    public DirectRewardListener(TaskCompletionSource<bool> tcs) => _tcs = tcs;

    //    public void OnUserEarnedReward(IRewardItem reward) => _tcs.TrySetResult(true);
    //}



    private void Button_Clicked(object sender, EventArgs e)
    {

    }








    private async void GoToNewStory_Clicked(object sender, EventArgs e)
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

        await Shell.Current.GoToAsync("//FairyTalePage");


        //#if ANDROID
        //        var result = await ShowAdSimple();

        //        if (result)
        //        {
        //            await DisplayAlert("Erfolg", "Belohnung erhalten!", "OK");
        //            await Shell.Current.GoToAsync("//FairyTalePage");
        //        }
        //        else
        //        {
        //            // Optional: Nachricht wenn Ad nicht geladen werden konnte
        //            bool weiter = await DisplayAlert("Ad Info", "Ad konnte nicht geladen werden. Trotzdem fortfahren?", "Ja", "Nein");
        //            if (weiter) await Shell.Current.GoToAsync("//FairyTalePage");
        //        }
        //#else
        //        await Shell.Current.GoToAsync("//FairyTalePage");
        //#endif
    }


    private void OnAdStatusChanged(object sender, string status)
    {
        Debug.WriteLine($"Ad Status: {status}");
        // Optional: UI aktualisieren
    }
}

#endif
