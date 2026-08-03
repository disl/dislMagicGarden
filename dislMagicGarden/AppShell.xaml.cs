using dislMagicGarden.Services;
using dislMagicGarden.Views;

namespace dislMagicGarden
{
    public partial class AppShell : Shell
    {
        private readonly AdService _adService;
        private readonly AiSettingsService _settings;
        private bool _firstRunHandled;
        private int _navigationCount;

        public AppShell(AdService adService, AiSettingsService settings)
        {
            InitializeComponent();

            _adService = adService;
            _settings = settings;

            SettingsShellContent.Title = AiSettingsService.T("Settings");

            // First-run: ask once for the AI provider when nothing is configured yet.
            Loaded += OnShellLoaded;

            Routing.RegisterRoute("HomePage", typeof(HomePage));
            Routing.RegisterRoute(nameof(ColoringGenerator), typeof(ColoringGenerator));
            Routing.RegisterRoute("NewStoryPage", typeof(FairyTalePage));
            Routing.RegisterRoute("AdventureHistoryPage", typeof(AdventureHistoryPage));
            Routing.RegisterRoute("SketchPage", typeof(SketchPage));
            Routing.RegisterRoute(nameof(QuizPage), typeof(QuizPage));

            // Navigation Events
            //this.Navigated += OnShellNavigated;
        }

        /// <summary>
        /// Shows a localized first-run prompt and opens the settings page when
        /// no AI provider has been configured yet.
        /// </summary>
        private async void OnShellLoaded(object? sender, EventArgs e)
        {
            if (_firstRunHandled || _settings.HasTextSettings)
                return;

            _firstRunHandled = true;

            bool go = await DisplayAlert(
                AiSettingsService.T("SettingsMissingTitle"),
                AiSettingsService.T("SettingsMissing"),
                AiSettingsService.T("SettingsOpen"),
                AiSettingsService.T("Cancel"));

            if (go)
                await Shell.Current.GoToAsync("//SettingsPage");
        }

        private async void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            //// Navigation zählen
            //_navigationCount++;

            //// Nach 2 Navigationen Ad zeigen
            //if (_navigationCount >= 2 && e.Source == ShellNavigationSource.ShellItemChanged)
            //{
            //    _navigationCount = 0;

            //    // Kleine Verzögerung
            //    await Task.Delay(800);

            //    // Ad versuchen zu zeigen
            //    await _adService.TryShowInterstitial();
            //}
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();

        //    // Initiale Ads laden
        //    _ = Task.Run(async () =>
        //    {
        //        await Task.Delay(5000); // 5 Sekunden nach Start
        //        await _adService.LoadInterstitialAsync();
        //    });
    }
}

