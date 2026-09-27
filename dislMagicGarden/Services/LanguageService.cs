using dislMagicGarden.Models;
using System.Globalization;

namespace dislMagicGarden.Services;

public interface ILanguageService
{
    string CurrentIso { get; set; }
    string LanguageName { get; }
    string Resolve(string iso);
    void SetSystemLanguage();
}

public class LanguageService : ILanguageService
{
    private readonly Dictionary<string, string> _map = new()
    {
        { "de", "German"  },
        { "en", "English" },
        { "fr", "French"  },
        { "es", "Spanish" },
        { "it", "Italian" },
        { "uk", "Ukrainian" },
        { "ru", "Russian" },
    };

    // Read on startup in App.xaml.cs; without a saved value the device language is used
    public const string PreferenceKeyAppLanguage = "app_language";

    /// <summary>
    /// App and story languages selectable in the settings.
    /// </summary>
    public static IReadOnlyList<LanguageOption> SupportedLanguages { get; } = new List<LanguageOption>
    {
        new() { Code = "en-US", DisplayName = "English (US)" },
        new() { Code = "de-DE", DisplayName = "Deutsch (DE)" },
        new() { Code = "fr-FR", DisplayName = "Français (FR)" },
        new() { Code = "es-ES", DisplayName = "Español (ES)" },
        new() { Code = "it-IT", DisplayName = "Italiano (IT)" },
        new() { Code = "uk-UA", DisplayName = "Українська (UA)" },
        new() { Code = "ru-RU", DisplayName = "Русский (RU)" },
    };

    /// <summary>
    /// Switches app and story language and remembers the choice for the next start.
    /// </summary>
    public static void SetAndSaveLanguage(string cultureCode)
    {
        SetLanguage(cultureCode);
        Preferences.Set(PreferenceKeyAppLanguage, cultureCode);
    }

    public static void SetLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);

        LocalizationResourceManager.Instance.SetCulture(culture); // Falls du einen Manager nutzt

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public string CurrentIso { get; set; } = "en";

    public string LanguageName => Resolve(CurrentIso);

    public string Resolve(string iso)
    {
        iso = iso.ToLower();

        return _map.ContainsKey(iso)
            ? _map[iso]
            : "English";
    }

    public void SetSystemLanguage()
    {
        try
        {
            // TEST !!!!!!!!

            //var culture = new CultureInfo("en-US");

            //CultureInfo.DefaultThreadCurrentCulture = culture;
            //CultureInfo.DefaultThreadCurrentUICulture = culture;

            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;


            //string iso = culture.TwoLetterISOLanguageName;

            //// Falls Sprache nicht unterstützt wird → Default DE
            //CurrentIso = _map.ContainsKey(iso)
            //    ? iso
            //    : "de";



            // AKTIVIEREN !!!!!!!!

            string iso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // Falls Sprache nicht unterstützt wird → Default DE
            CurrentIso = _map.ContainsKey(iso)
                ? iso
                : "en";
        }
        catch
        {
            CurrentIso = "de";
        }
    }
}
