using dislMagicGarden.Services;
using dislMagicGarden.ViewModels;
using dislMagicGarden.Views;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Reflection;

namespace dislMagicGarden
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 2. appsettings.json als Embedded Resource auslesen und zur Konfiguration hinzufügen
            var a = Assembly.GetExecutingAssembly();
            var res_names = a.GetManifestResourceNames();
            using var stream = a.GetManifestResourceStream(@"dislMagicGarden.appsettings.json");

            // !!! WICHTIG: Ersetzen Sie "IhrProjektnamensraum" durch den tatsächlichen Standard-Namespace Ihres Projekts.
            // Beispiel: "MauiApp1.appsettings.json"

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream) // Diese Erweiterungsmethode ist in Microsoft.Extensions.Configuration.Json verfügbar
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }




#if DEBUG
            //builder.Logging.AddDebug();
#endif

            // ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<FairyTaleViewModel>();

            // Views
            builder.Services.AddTransient<HomePage>();
            //builder.Services.AddTransient<NewStoryPage>();
            builder.Services.AddTransient<FairyTalePage>();


            // Services (noch nicht implementiert)
            builder.Services.AddSingleton<IStoryService, StoryService>();
            builder.Services.AddSingleton<Models.IIllustrationService, IllustrationService>();
            //builder.Services.AddSingleton<IEditorService, EditorService>();
            builder.Services.AddSingleton<IBookExportService, BookExportService>();
            builder.Services.AddSingleton<ILanguageService, LanguageService>();
            builder.Services.AddSingleton<IHybridFairyTaleService, HybridFairyTaleService>();

            //builder.Services.AddTransient<DeepSeekClient>();

    

            builder.Services.AddLocalization();


            var app = builder.Build();

            // Culture Einstellungen
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

            return app;


        }
    }
}
