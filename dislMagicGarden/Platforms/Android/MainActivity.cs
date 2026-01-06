using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Gms.Ads.Initialization;
using Android.OS;

namespace dislMagicGarden
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MobileAds.Initialize(this, new OnInitializationCompleteListener());

            //Android.Gms.Ads.MobileAds.Initialize(this);
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
