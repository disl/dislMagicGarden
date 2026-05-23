using dislMagicGarden.Services;
using System.Globalization;

namespace dislMagicGarden
{
    public partial class App : Application
    {
        private readonly ILanguageService _language;
        private readonly AdService _adService;

        public App(ILanguageService languageService, AdService adService)
        {
            InitializeComponent();

            _language = languageService;
            _adService = adService;

            ApplyStartupCulture();

            // Sprache automatisch vom Gerät übernehmen
            _language.SetSystemLanguage();

            var curr_cult = CultureInfo.CurrentUICulture.Name;

            var lang = Preferences.Get("app_language", curr_cult);
            LanguageService.SetLanguage(lang);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(_adService));
        }

        private void ApplyStartupCulture()
        {
            // gespeicherte Sprache (z. B. "en-US", "de-DE")
            var savedLanguage = Preferences.Get("AppLanguage", string.Empty);

            CultureInfo culture;

            if (!string.IsNullOrWhiteSpace(savedLanguage))
            {
                culture = new CultureInfo(savedLanguage);
            }
            else
            {
                // Fallback: Gerätesprache
                culture = CultureInfo.CurrentUICulture;
            }

            SetCulture(culture);
        }

        private void SetCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }




//        public static  void Reload()
//        {
//            MainThread.BeginInvokeOnMainThread(() =>
//            {
//#if ANDROID
//                var currentActivity = Platform.CurrentActivity;

//                Current.MainPage = new AppShell(_adService);

//#endif
//            });
//        }
    }

}