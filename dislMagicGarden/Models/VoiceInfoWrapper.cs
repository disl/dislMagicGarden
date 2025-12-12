using MauiLocale = Microsoft.Maui.Media.Locale;

namespace dislMagicGarden.Models
{
    //public class Locale
    //{
    //    public string Language { get; }       // z. B. "de", "en"
    //    public string Country { get; }        // z. B. "DE", "US"
    //    public string DisplayName { get; }    // Benutzerfreundliche Anzeige
    //}

    //public class VoiceInfo
    //{
    //    public string Name { get; }        // Geräteabhängiger Stimm-Name — z. B. "de-DE-XeniaNeural"
    //    public Locale Locale { get; }      // Sprach- und Regionscode: z. B. "de-DE"
    //}


    public class LocaleWrapper
    {
        public MauiLocale Locale { get; set; }

        public string DisplayName => $"{Locale.Name}";

        private string GetLanguageName(string langCode) =>
            langCode switch
            {
                "de" => "Deutsch",
                "en" => "Englisch",
                "fr" => "Französisch",
                "es" => "Spanisch",
                "it" => "Italienisch",
                "ru" => "Russisch",
                _ => langCode
            };
    }
}
