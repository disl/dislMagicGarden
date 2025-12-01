using dislMagicGarden.Services;

namespace dislMagicGarden
{
    public partial class App : Application
    {
        private readonly ILanguageService _language;

        public App(ILanguageService languageService)
        {
            InitializeComponent();

            _language = languageService;

            // Sprache automatisch vom Gerät übernehmen
            _language.SetSystemLanguage();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }

}