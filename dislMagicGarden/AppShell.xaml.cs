using dislMagicGarden.Services;
using dislMagicGarden.Views;

namespace dislMagicGarden
{
    public partial class AppShell : Shell
    {
        private readonly AdService _adService;
        private int _navigationCount;

        public AppShell(AdService adService)
        {
            InitializeComponent();

            _adService = adService;

            Routing.RegisterRoute("HomePage", typeof(HomePage));
            Routing.RegisterRoute(nameof(ColoringGenerator), typeof(ColoringGenerator));
            Routing.RegisterRoute("NewStoryPage", typeof(FairyTalePage));

            // Navigation Events
            //this.Navigated += OnShellNavigated;
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

