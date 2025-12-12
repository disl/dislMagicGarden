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
        { "ru", "Russian" },
    };

    public static void SetLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public string CurrentIso { get; set; } = "de";

    public string LanguageName => Resolve(CurrentIso);

    public string Resolve(string iso)
    {
        iso = iso.ToLower();

        return _map.ContainsKey(iso)
            ? _map[iso]
            : "German";
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
