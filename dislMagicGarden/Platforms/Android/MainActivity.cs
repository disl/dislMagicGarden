using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;

namespace dislMagicGarden
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Initialisierung der Google Play Services Ads
            Android.Gms.Ads.MobileAds.Initialize(this);
        }

        //public override void OnAdFailedToLoad(LoadAdError error)
        //{
        //    base.OnAdFailedToLoad(error);
        //    // Schau in das Output-Fenster von Visual Studio:
        //    System.Diagnostics.Debug.WriteLine($"AdMob Error Code: {error.Code} - Message: {error.Message}");
        //}
    }



}
