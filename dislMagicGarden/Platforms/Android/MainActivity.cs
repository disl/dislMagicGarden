using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Gms.Ads.Initialization;
using Android.OS;
using Plugin.MauiMtAdmob;

namespace dislMagicGarden
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // AdMob initialisieren – HIER die App ID angeben!
            CrossMauiMTAdmob.Current.Init(
                activity: this,                                           // Pflicht: die aktuelle Activity
                appId: "ca-app-pub-3940256099942544~3347511713",          // Test-App-ID (später deine echte)
                forceTesting: true,                                       // Optional: Test-Ads erzwingen
                debugMode: true                                           // Optional: Logs aktivieren
            // weitere optionale Parameter wie license, openAdsId usw. bei Bedarf
            );
        }

        public class OnInitializationCompleteListener : Java.Lang.Object, IOnInitializationCompleteListener
        {
            public void OnInitializationComplete(IInitializationStatus status)
            {
                Console.WriteLine("DEBUG_AD: Google SDK ist jetzt wirklich bereit!");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }



}
