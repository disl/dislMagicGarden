#if IOS || MACCATALYST
using UIKit;
using dislMagicGarden.Controls;
using Microsoft.Maui.Handlers;

namespace dislMagicGarden.Handlers
{
    public class AdBannerViewHandler : ViewHandler<AdBannerView, UIView>
    {
        public static IPropertyMapper<AdBannerView, IViewHandler> Mapper =
            new PropertyMapper<AdBannerView, IViewHandler>(ViewMapper);

        public AdBannerViewHandler() : base(Mapper)
        {
        }

        public AdBannerViewHandler(IPropertyMapper mapper) : base(mapper)
        {
        }

        protected override UIView CreatePlatformView()
        {
            var view = new UIView
            {
                BackgroundColor = UIColor.SystemGray6
            };

            var label = new UILabel
            {
                Text = "Ad Banner (iOS)",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.SystemGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            view.AddSubview(label);

            // Center constraints
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                label.CenterXAnchor.ConstraintEqualTo(view.CenterXAnchor),
                label.CenterYAnchor.ConstraintEqualTo(view.CenterYAnchor),
                label.WidthAnchor.ConstraintEqualTo(view.WidthAnchor, multiplier: 0.9f)
            });

            return view;
        }
    }
}
#endif