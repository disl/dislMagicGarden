#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using dislMagicGarden.Controls;
using Microsoft.Maui.Handlers;
using Border = Microsoft.UI.Xaml.Controls.Border;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using Color = Windows.UI.Color;
using Thickness = Microsoft.UI.Xaml.Thickness;
using CornerRadius = Microsoft.UI.Xaml.CornerRadius;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace dislMagicGarden.Handlers
{
    public class AdBannerViewHandler : ViewHandler<AdBannerView, Border>
    {
        public static IPropertyMapper<AdBannerView, IViewHandler> Mapper =
            new PropertyMapper<AdBannerView, IViewHandler>(ViewMapper);

        public AdBannerViewHandler() : base(Mapper)
        {
        }

        public AdBannerViewHandler(IPropertyMapper mapper) : base(mapper)
        {
        }

        protected override Microsoft.UI.Xaml.Controls.Border CreatePlatformView()
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var textBlock = new TextBlock
            {
                Text = "Ad Banner (Windows)",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120))
            };

            border.Child = textBlock;
            return border;
        }
    }
}
#endif