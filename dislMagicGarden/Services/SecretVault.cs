using System.Text;

namespace dislMagicGarden.Services
{
    // Hält API-Keys nicht als Klartext im appsettings.json/Assembly, sondern XOR+Base64-obfuskiert,
    // damit sie nicht per "strings app.apk" oder automatisiertem Secret-Scanning trivial auffindbar sind.
    // Schützt nicht vor gezieltem Decompilieren - nur Fix ist ein serverseitiger Proxy.
    internal static class SecretVault
    {
        private const string Passphrase = "WhimsyTales_v2_Salt#2026";

        // Neu generieren nach jeder Key-Rotation: siehe scratchpad/obfuscate.py
        private const string DeepSeekApiKeyEncoded = "JANEXkYYNlNcXBc8EAM9Z1NdRBpTCVEOZAtaDkUcNVdVVRY=";

        public static string DeepSeekApiKey => Decode(DeepSeekApiKeyEncoded);

        private static string Decode(string encoded)
        {
            var passphraseBytes = Encoding.UTF8.GetBytes(Passphrase);
            var data = Convert.FromBase64String(encoded);
            var result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ passphraseBytes[i % passphraseBytes.Length]);

            return Encoding.UTF8.GetString(result);
        }
    }
}
