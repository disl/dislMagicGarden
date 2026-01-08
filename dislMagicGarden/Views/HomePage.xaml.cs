#if ANDROID
using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
#endif

using dislMagicGarden.ViewModels;
using System.Diagnostics;

namespace dislMagicGarden.Views;

public partial class HomePage : FairyBasePage
{
    static bool m_need_for_update = true;
    private static bool _isAdLoading;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }

    protected async override void OnAppearing()
    {
        if (m_need_for_update)
        {
#if ANDROID
            if (IsPlayCoreApiAvailable())
            {
                await CheckForUpdates();
            }
#endif
            m_need_for_update = false;
        }
    }


#if ANDROID
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


#if ANDROID


    private static async Task<bool> ShowAdSimple()
    {
        if (_isAdLoading) return false;
        _isAdLoading = true;

        try
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return false;

            var tcs = new TaskCompletionSource<bool>();
            var callback = new DirectCallback(tcs);

            // Zwinge den Aufruf auf den Main Thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var request = new AdRequest.Builder().Build();
                    RewardedAd.Load(activity, "ca-app-pub-3940256099942544/1033173712", request, callback);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fehler beim Load-Start: {ex.Message}");
                    tcs.TrySetResult(false);
                }
            });

            // Dein Timeout-Code
            var timeoutTask = Task.Delay(15000);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            return completedTask == tcs.Task ? await tcs.Task : false;
        }
        finally
        {
            _isAdLoading = false;
        }
    }

    // Minimale Callback-Klasse
    public class DirectCallback : RewardedAdLoadCallback
    {
        private TaskCompletionSource<bool> _tcs;

        public DirectCallback(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public virtual void OnAdLoaded(RewardedAd rewardedAd)
        {
            Debug.WriteLine("DEBUG_AD: Ad geladen!");
            rewardedAd.FullScreenContentCallback = new DirectFullScreenCallback(_tcs);
            rewardedAd.Show(Platform.CurrentActivity, new DirectRewardListener(_tcs));
        }

        public override void OnAdFailedToLoad(LoadAdError error)
        {
            Debug.WriteLine($"DEBUG_AD: Fehler beim Laden: {error.Message}");
            _tcs.TrySetResult(false);
        }
    }

    public class DirectFullScreenCallback : FullScreenContentCallback
    {
        private TaskCompletionSource<bool> _tcs;

        public DirectFullScreenCallback(TaskCompletionSource<bool> tcs) => _tcs = tcs;

        public override void OnAdDismissedFullScreenContent() => _tcs.TrySetResult(false);
    }

    public class DirectRewardListener : Java.Lang.Object, IOnUserEarnedRewardListener
    {
        private TaskCompletionSource<bool> _tcs;

        public DirectRewardListener(TaskCompletionSource<bool> tcs) => _tcs = tcs;

        public void OnUserEarnedReward(IRewardItem reward) => _tcs.TrySetResult(true);
    }

    

    private void Button_Clicked(object sender, EventArgs e)
    {

    }
#endif




#endif


    private async void GoToNewStory_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("------- GoToNewStory_Clicked - Start");

        //var result = await ShowAdSimple();

        //if (result)
        //{
        //    await DisplayAlert("Erfolg", "Belohnung erhalten!", "OK");
        //    // Nur wenn das Ad erfolgreich war, zur neuen Seite
        await Shell.Current.GoToAsync("//FairyTalePage");
        //}
        //else
        //{
        //    // Optional: Nachricht wenn Ad nicht geladen werden konnte
        //    bool weiter = await DisplayAlert("Ad Info", "Ad konnte nicht geladen werden. Trotzdem fortfahren?", "Ja", "Nein");
        //    if (weiter) await Shell.Current.GoToAsync("//FairyTalePage");
        //}
    }

}