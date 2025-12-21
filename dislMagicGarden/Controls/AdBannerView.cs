using Microsoft.Maui.Controls;

namespace dislMagicGarden.Controls
{
    // Stellen Sie sicher, dass es von View erbt
    public class AdBannerView : View
    {
        public static readonly BindableProperty AdUnitIdProperty =
            BindableProperty.Create(
                nameof(AdUnitId),
                typeof(string),
                typeof(AdBannerView),
                string.Empty);

        public string AdUnitId
        {
            get => (string)GetValue(AdUnitIdProperty);
            set => SetValue(AdUnitIdProperty, value);
        }
    }
}