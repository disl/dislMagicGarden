#if ANDROID
using Android.Gms.Ads;
using dislMagicGarden.Controls;
using Microsoft.Maui.Handlers;
using AdRequest = Android.Gms.Ads.AdRequest;


namespace dislMagicGarden.Handlers
{
    public class AdBannerViewHandler : ViewHandler<AdBannerView, AdView>
    {
        //public static IPropertyMapper<AdBannerView, IViewHandler> Mapper =
        //    new PropertyMapper<AdBannerView, IViewHandler>(ViewMapper);

        public AdBannerViewHandler() : base(ViewMapper)
        {
        }

        public static IPropertyMapper<AdBannerView, AdBannerViewHandler> ViewMapper = new PropertyMapper<AdBannerView, AdBannerViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(AdBannerView.AdUnitId)] = MapAdUnitId
        };

        protected override AdView CreatePlatformView()
        {
            var adView = new AdView(Context);
            adView.AdSize = AdSize.Banner;
            adView.AdUnitId = VirtualView.AdUnitId;
            adView.LoadAd(new AdRequest.Builder().Build());
            return adView;


            // Einfacher TextView als Platzhalter
            //return new AppCompatTextView(Context)
            //{
            //    Text = "Ad Banner (Android)",
            //    Gravity = Android.Views.GravityFlags.Center,
            //    LayoutParameters = new ViewGroup.LayoutParams(
            //        ViewGroup.LayoutParams.MatchParent,
            //        50) // Feste Höhe
            //};
        }

        public static void MapAdUnitId(AdBannerViewHandler handler, AdBannerView view)
        {
            if (handler.PlatformView != null)
            {
                //handler.PlatformView.AdUnitId = view.AdUnitId;
                handler.PlatformView.LoadAd(new AdRequest.Builder().Build());
            }
        }

        //protected override void ConnectHandler(AppCompatTextView platformView)
        //{
        //    base.ConnectHandler(platformView);

        //    // Hier können Sie später Ad-Logik hinzufügen
        //    if (VirtualView != null && !string.IsNullOrEmpty(VirtualView.AdUnitId))
        //    {
        //        // Ad initialisieren
        //    }
        //}
    }
}
#endif