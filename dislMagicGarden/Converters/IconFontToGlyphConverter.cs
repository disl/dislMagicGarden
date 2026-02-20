using dislMagicGarden.Helpers;
using System.Globalization;

namespace dislMagicGarden.Converters
{
    public class IconFontToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string glyphString)
            {
                // Falls schon "\ue3f4" oder ähnlich → direkt zurück
                if (glyphString.StartsWith("\\u") || glyphString.StartsWith(@"\u"))
                {
                    return glyphString;
                }

                // Fallback: wir versuchen es als Member-Name zu interpretieren
                // (wenn man den Converter mit x:Static nutzt, kommt meist der String-Name rein)
                return GetGlyphFromName(glyphString);
            }

            // Alternativ: Enum, Icon-Objekt, ... → hier erweiterbar
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetGlyphFromName(string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
                return ""; // ungültiges Zeichen

            // Reflexion – am einfachsten, aber etwas langsamer
            var type = typeof(IconFont);
            var field = type.GetField(iconName);

            if (field != null && field.IsLiteral && field.FieldType == typeof(string))
            {
                return (string)field.GetRawConstantValue();
            }

            // Fallback – entweder Exception oder leeres Zeichen
            // return "";
            throw new ArgumentException($"Icon '{iconName}' nicht in IconFont gefunden.");
        }

        // Hilfsmethode – falls du den Converter statisch nutzen willst
        public static string GetGlyph(string iconName) => GetGlyphFromName(iconName);
    }
}
