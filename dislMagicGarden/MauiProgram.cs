using dislMagicGarden.Models;
using dislMagicGarden.Services;
using dislMagicGarden.ViewModels;
using dislMagicGarden.Views;
using Microsoft.Extensions.Logging;
using System.Globalization;

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

#if DEBUG
    		builder.Logging.AddDebug();
#endif


            // ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<NewStoryViewModel>();

            // Views
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<NewStoryPage>();


            // Services (noch nicht implementiert)
            builder.Services.AddSingleton<IStoryService, StoryService>();
            builder.Services.AddSingleton<Models.IIllustrationService, IllustrationService>();
            builder.Services.AddSingleton<IEditorService, EditorService>();
            builder.Services.AddSingleton<IBookExportService, BookExportService>();
            builder.Services.AddSingleton<ILanguageService, LanguageService>();

            builder.Services.AddLocalization();


            var app = builder.Build();

            // Culture Einstellungen
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

            return app;


        }
    }
}
