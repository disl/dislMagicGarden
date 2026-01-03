using dislMagicGarden.Views;

namespace dislMagicGarden
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("NewStoryPage", typeof(FairyTalePage));
            Routing.RegisterRoute(nameof(ColoringGenerator), typeof(ColoringGenerator));
        }
    }
}
