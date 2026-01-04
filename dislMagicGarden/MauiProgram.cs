using CommunityToolkit.Maui;
using dislMagicGarden.Controls;
using dislMagicGarden.Handlers;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using dislMagicGarden.Services.dislMagicGarden.Services;
using dislMagicGarden.ViewModels;
using dislMagicGarden.Views;
using Microsoft.Extensions.Configuration;
using PdfSharp.Fonts;
using System.Globalization;
using System.Reflection;

namespace dislMagicGarden
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                // ZUERST: FontResolver registrieren (vor allem anderen)
                GlobalFontSettings.FontResolver = new MyFontResolver();

                // ODER mit der einfachen Version:
                // GlobalFontSettings.FontResolver = SimpleFontResolver.Instance;

                Console.WriteLine("PDFSharp FontResolver initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR initializing FontResolver: {ex.Message}");

                // Fallback: Leeren FontResolver verwenden
                GlobalFontSettings.FontResolver = new FallbackFontResolver();
            }

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 2. appsettings.json als Embedded Resource auslesen und zur Konfiguration hinzufügen
            var a = Assembly.GetExecutingAssembly();
            var res_names = a.GetManifestResourceNames();
            using var stream = a.GetManifestResourceStream(@"dislMagicGarden.appsettings.json");

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream) // Diese Erweiterungsmethode ist in Microsoft.Extensions.Configuration.Json verfügbar
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }

            // Handler registrieren - Jede Plattform bekommt ihren eigenen
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<AdBannerView, AdBannerViewHandler>();
            });

#if ANDROID
            builder.Services.AddSingleton<IRewardedAdService, dislMagicGarden.Platforms.Android.RewardedAdService>();
#endif


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

            builder.Services.AddHttpClient<ImageGeneratorService>();

            //builder.Services.AddTransient<DeepSeekClient>();

            builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();

            builder.Services.AddLocalization();


            var app = builder.Build();

            // Culture Einstellungen
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

           

            return app;


        }
    }

    public class FallbackFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName) => Array.Empty<byte>();

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            => new FontResolverInfo("Arial");
    }
}
