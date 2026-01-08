using Android.App;
using Android.Gms.Ads;
using Android.Runtime;

namespace dislMagicGarden
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            //MobileAds.Initialize(this);

            // Firebase muss initialisiert werden
            //FirebaseApp.InitializeApp(this);
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
