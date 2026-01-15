using dislMagicGarden.Properties;
using System.ComponentModel;
using System.Globalization;

namespace dislMagicGarden
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        public static LocalizationResourceManager Instance { get; } = new();

        // Dein Standard-Resource-File (z.B. Resources/AppResources.resx)
        public string this[string resourceKey] =>
            Resources.ResourceManager.GetString(resourceKey, Resources.Culture);

        public void SetCulture(CultureInfo culture)
        {
            Resources.Culture = culture;
            // WICHTIG: Benachrichtigt alle Bindings, dass sich die Texte geändert haben
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }



    
}
