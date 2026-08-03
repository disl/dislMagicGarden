using System.Globalization;

namespace dislMagicGarden.Services;

/// <summary>
/// Immutable configuration for the text/LLM provider (stories).
/// </summary>
public sealed record AiTextSettings(string Provider, string ApiKey, string BaseUrl, string Model)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(BaseUrl);
}

/// <summary>
/// Immutable configuration for the image generation provider (coloring pages).
/// </summary>
public sealed record AiImageSettings(string Provider, string ApiKey, string BaseUrl, string Model)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(BaseUrl);
}

/// <summary>
/// Stores and loads the AI provider configuration (text + image) for the app.
/// API keys are kept in SecureStorage (device-encrypted), everything else in Preferences.
/// </summary>
public class AiSettingsService
{
    // ── Text provider presets ──
    public const string TextProviderDeepSeek = "DeepSeek";
    public const string TextProviderOpenRouter = "OpenRouter";
    public const string TextProviderCustom = "Custom";
    public const string DeepSeekBaseUrl = "https://api.deepseek.com/v1";
    public const string DeepSeekModel = "deepseek-chat";
    public const string DeepSeekKeysUrl = "https://platform.deepseek.com/api_keys";
    public const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1";
    public const string OpenRouterDefaultModel = "deepseek/deepseek-chat";
    public const string OpenRouterKeysUrl = "https://openrouter.ai/keys";
    public const string CustomKeysUrl = "https://platform.openai.com/api-keys";

    // ── Image provider presets ──
    public const string ImageProviderTogether = "Together";
    public const string ImageProviderOpenAi = "OpenAI";
    public const string ImageProviderCustom = "Custom";
    public const string TogetherBaseUrl = "https://api.together.xyz/v1";
    public const string TogetherModel = "black-forest-labs/FLUX.1-schnell";
    public const string TogetherKeysUrl = "https://api.together.xyz/settings/api-keys";
    public const string OpenAiBaseUrl = "https://api.openai.com/v1";
    public const string OpenAiImageModel = "gpt-image-1";
    public const string OpenAiKeysUrl = "https://platform.openai.com/api-keys";

    // ── Text persistence keys ──
    private const string PrefTextProvider = "ai_text_provider";
    private const string PrefTextBaseUrl = "ai_text_baseurl";
    private const string PrefTextModel = "ai_text_model";
    private const string PrefTextSaved = "ai_text_saved";
    private const string SecureTextKey = "ai_text_apikey";

    // ── Image persistence keys ──
    private const string PrefImageProvider = "ai_image_provider";
    private const string PrefImageBaseUrl = "ai_image_baseurl";
    private const string PrefImageModel = "ai_image_model";
    private const string PrefImageSaved = "ai_image_saved";
    private const string SecureImageKey = "ai_image_apikey";

    /// <summary>True once a text connection test succeeded (first-run dialog should not re-appear).</summary>
    public bool HasTextSettings => Preferences.Default.Get(PrefTextSaved, false);

    /// <summary>True once an image connection test succeeded.</summary>
    public bool HasImageSettings => Preferences.Default.Get(PrefImageSaved, false);

    public async Task<AiTextSettings> LoadTextAsync()
    {
        var provider = Preferences.Default.Get(PrefTextProvider, TextProviderDeepSeek);
        var baseUrl = Preferences.Default.Get(PrefTextBaseUrl, DeepSeekBaseUrl);
        var model = Preferences.Default.Get(PrefTextModel, DeepSeekModel);
        var apiKey = await SecureStorage.Default.GetAsync(SecureTextKey) ?? "";

        return new AiTextSettings(provider, apiKey, baseUrl, model);
    }

    public async Task<AiImageSettings> LoadImageAsync()
    {
        var provider = Preferences.Default.Get(PrefImageProvider, ImageProviderTogether);
        var baseUrl = Preferences.Default.Get(PrefImageBaseUrl, TogetherBaseUrl);
        var model = Preferences.Default.Get(PrefImageModel, TogetherModel);
        var apiKey = await SecureStorage.Default.GetAsync(SecureImageKey) ?? "";

        return new AiImageSettings(provider, apiKey, baseUrl, model);
    }

    public async Task SaveTextAsync(AiTextSettings settings)
    {
        Preferences.Default.Set(PrefTextProvider, settings.Provider);
        Preferences.Default.Set(PrefTextBaseUrl, settings.BaseUrl);
        Preferences.Default.Set(PrefTextModel, settings.Model);
        Preferences.Default.Set(PrefTextSaved, true);

        await StoreKeyAsync(SecureTextKey, settings.ApiKey);
    }

    public async Task SaveImageAsync(AiImageSettings settings)
    {
        Preferences.Default.Set(PrefImageProvider, settings.Provider);
        Preferences.Default.Set(PrefImageBaseUrl, settings.BaseUrl);
        Preferences.Default.Set(PrefImageModel, settings.Model);
        Preferences.Default.Set(PrefImageSaved, true);

        await StoreKeyAsync(SecureImageKey, settings.ApiKey);
    }

    private static async Task StoreKeyAsync(string secureKey, string apiKey)
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
            await SecureStorage.Default.SetAsync(secureKey, apiKey);
        else
            SecureStorage.Default.Remove(secureKey);
    }

    /// <summary>
    /// Localized resource lookup without depending on the (IDE-generated) Designer class.
    /// Falls back to the key itself when the resource is missing.
    /// </summary>
    public static string T(string key)
    {
        var culture = CultureInfo.CurrentCulture;
        var text = global::dislMagicGarden.Properties.Resources.ResourceManager.GetString(key, culture);
        return string.IsNullOrEmpty(text) ? key : text;
    }
}
