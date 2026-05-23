using Android.App;
using Android.Content.PM;
using Android.Gms.Ads.Initialization;
using Android.OS;
using Plugin.MauiMTAdmob;

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
                appId: "ca-app-pub-9459821903521146~7288668937",          // Test-App-ID (später deine echte)
                forceTesting: true,                                       // Optional: Test-Ads erzwingen
                debugMode: true                                           // Optional: Logs aktivieren
                                                                          // weitere optionale Parameter wie license, openAdsId usw. bei Bedarf            
            );

            // Optional: Set user consent if needed
            //CrossMauiMTAdmob.Current.UserPersonalizedAds = true;

            // AdService erst jetzt starten
            //try
            //{
            //    var adService = Application.Current?.Services?.GetService<dislMagicGarden.Services.AdService>();
            //    adService?.Start();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"[MainActivity] AdService Start Fehler: {ex.Message}");
            //}
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
