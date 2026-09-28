using Android.App;
using Android.Content.PM;
using Android.Gms.Ads.Initialization;
using Android.OS;
using Plugin.MauiMTAdmob;
using Plugin.MauiMTAdmob.Extra;

namespace dislMagicGarden
{
    // MagicGardenTheme extends Maui.SplashTheme and only adds the light/dark status bar
    // icon flags (see Platforms/Android/Resources/values(-night)/styles.xml).
    [Activity(Theme = "@style/MagicGardenTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Kinder-App (Google Play Families Policy / COPPA / DSGVO Art. 8):
            // muss VOR Init gesetzt werden, damit jede Anzeigenanfrage kindgerecht markiert ist.
            CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
            CrossMauiMTAdmob.Current.TagForChildDirectedTreatment = MTTagForChildDirectedTreatment.TagForChildDirectedTreatmentTrue;
            CrossMauiMTAdmob.Current.TagForUnderAgeOfConsent = MTTagForUnderAgeOfConsent.TagForUnderAgeOfConsentTrue;
            CrossMauiMTAdmob.Current.MaxAdContentRating = MTMaxAdContentRating.MaxAdContentRatingG;
            CrossMauiMTAdmob.Current.UserPersonalizedAds = false;

#if DEBUG
            const bool adTesting = true;
#else
            const bool adTesting = false;
#endif
            // AdMob initialisieren (App ID "Whimsy Tales", muss zum Manifest passen)
            CrossMauiMTAdmob.Current.Init(
                activity: this,
                appId: "ca-app-pub-9459821903521146~7288668937",
                forceTesting: adTesting,                                  // Test-Ads nur im Debug
                debugMode: adTesting                                      // Logs nur im Debug
            );

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
