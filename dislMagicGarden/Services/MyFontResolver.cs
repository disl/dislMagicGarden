using PdfSharp.Fonts;

namespace dislMagicGarden.Services
{
    using PdfSharp.Fonts;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    namespace dislMagicGarden.Services
    {
        public class MyFontResolver : IFontResolver
        {
            private static readonly Lazy<MyFontResolver> _instance =
                new Lazy<MyFontResolver>(() => new MyFontResolver());

            private readonly Dictionary<string, byte[]> _fonts = new();

            public static MyFontResolver Instance => _instance.Value;

            public MyFontResolver()
            {
                // Fonts aus Embedded Resources laden
                LoadFontFromResources();
            }

            public byte[] GetFont(string faceName)
            {
                var key = faceName.Replace(".ttf", "");
                return _fonts.TryGetValue(key, out var fontData)
                    ? fontData
                    : _fonts["OpenSans-Regular"]; // Fallback
            }

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                return new FontResolverInfo(isBold ? "OpenSans-Semibold" : "OpenSans-Regular");
            }

            private void LoadFontFromResources()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName().Name;

                // WICHTIG: Diese Resource-Namen müssen mit den tatsächlichen übereinstimmen
                // Fügen Sie Debug-Ausgabe hinzu, um die richtigen Namen zu finden
                var fontResources = new Dictionary<string, string>
                {
                    ["OpenSans-Regular"] = $"{assemblyName}.Resources.Fonts.OpenSans-Regular.ttf",
                    ["OpenSans-Semibold"] = $"{assemblyName}.Resources.Fonts.OpenSans-Semibold.ttf"
                };

                foreach (var font in fontResources)
                {
                    var stream = assembly.GetManifestResourceStream(font.Value);

                    if (stream == null)
                    {
                        // Alternative suchen
                        var altName = assembly.GetManifestResourceNames()
                            .FirstOrDefault(r => r.Contains(font.Key) && r.EndsWith(".ttf"));

                        if (altName != null)
                            stream = assembly.GetManifestResourceStream(altName);
                    }

                    if (stream != null)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        _fonts[font.Key] = ms.ToArray();
                    }
                    else
                    {
                        // Font nicht gefunden - mit Standard-Fallback
                        _fonts[font.Key] = CreateFallbackFont();
                    }
                }
            }

            private byte[] CreateFallbackFont()
            {
                // Ein minimaler Fallback-Font (Arial-ähnlich)
                // Oder: Lade eine im Code eingebettete minimale TTF
                return LoadMinimalFont();
            }

            private byte[] LoadMinimalFont()
            {
                // Hier können Sie einen minimalen Font als byte[] einbetten
                // oder eine System-Schriftart verwenden
                return System.Text.Encoding.UTF8.GetBytes("MINIMAL_FONT_PLACEHOLDER");
            }
        }
    }
}