using System.Globalization;
using dislMagicGarden.Services;

namespace dislMagicGarden.Views;

public partial class SettingsPage : ContentPage
{
    private readonly AiSettingsService _settings;
    private readonly IHybridFairyTaleService _fairyTaleService;
    private readonly ImageGeneratorService _imageService;
    private bool _savingText;
    private bool _savingImage;
    private string? _textKeysUrl;
    private string? _imageKeysUrl;

    public SettingsPage(AiSettingsService settings, IHybridFairyTaleService fairyTaleService, ImageGeneratorService imageService)
    {
        InitializeComponent();
        _settings = settings;
        _fairyTaleService = fairyTaleService;
        _imageService = imageService;

        TextProviderPicker.ItemsSource = new[]
        {
            AiSettingsService.T("ProviderDeepSeek"),
            AiSettingsService.T("ProviderOpenRouter"),
            AiSettingsService.T("ProviderCustom")
        };

        ImageProviderPicker.ItemsSource = new[]
        {
            AiSettingsService.T("ProviderTogether"),
            AiSettingsService.T("ProviderOpenAi"),
            AiSettingsService.T("ProviderCustom")
        };
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        BackBtn.Text = "←  " + AiSettingsService.T("Back");
        TextSectionTitle.Text = AiSettingsService.T("SettingsTextSection");
        TextProviderLabel.Text = AiSettingsService.T("SettingsProviderLabel");
        TextApiKeyLabel.Text = AiSettingsService.T("SettingsApiKeyLabel");
        TextBaseUrlLabel.Text = AiSettingsService.T("SettingsBaseUrlLabel");
        TextModelLabel.Text = AiSettingsService.T("SettingsModelLabel");
        TextSaveBtn.Text = AiSettingsService.T("SettingsSaveAndTest");

        ImageSectionTitle.Text = AiSettingsService.T("SettingsImageSection");
        ImageProviderLabel.Text = AiSettingsService.T("SettingsProviderLabel");
        ImageApiKeyLabel.Text = AiSettingsService.T("SettingsApiKeyLabel");
        ImageBaseUrlLabel.Text = AiSettingsService.T("SettingsBaseUrlLabel");
        ImageModelLabel.Text = AiSettingsService.T("SettingsModelLabel");
        ImageSaveBtn.Text = AiSettingsService.T("SettingsSaveAndTest");

        var text = await _settings.LoadTextAsync();
        TextProviderPicker.SelectedIndex = TextProviderToIndex(text.Provider);
        TextApiKeyEntry.Text = text.ApiKey;
        TextBaseUrlEntry.Text = text.BaseUrl;
        TextModelEntry.Text = text.Model;

        var image = await _settings.LoadImageAsync();
        ImageProviderPicker.SelectedIndex = ImageProviderToIndex(image.Provider);
        ImageApiKeyEntry.Text = image.ApiKey;
        ImageBaseUrlEntry.Text = image.BaseUrl;
        ImageModelEntry.Text = image.Model;

        OnTextProviderChanged(null, EventArgs.Empty);
        OnImageProviderChanged(null, EventArgs.Empty);
    }

    // ── Text provider ──
    private void OnTextProviderChanged(object? sender, EventArgs e)
    {
        (string linkKey, string? baseUrl, string? model, string keysUrl) = TextProviderPicker.SelectedIndex switch
        {
            1 => ("ApiKeyLinkOpenRouter", AiSettingsService.OpenRouterBaseUrl, AiSettingsService.OpenRouterDefaultModel, AiSettingsService.OpenRouterKeysUrl),
            2 => ("ApiKeyLinkCustom", null, null, AiSettingsService.CustomKeysUrl),
            _ => ("ApiKeyLinkDeepSeek", AiSettingsService.DeepSeekBaseUrl, AiSettingsService.DeepSeekModel, AiSettingsService.DeepSeekKeysUrl)
        };

        TextApiKeyLinkLabel.Text = AiSettingsService.T(linkKey);
        _textKeysUrl = keysUrl;

        if (baseUrl is not null) TextBaseUrlEntry.Text = baseUrl;
        if (model is not null) TextModelEntry.Text = model;
    }

    private async void OnTextApiKeyLinkTapped(object? sender, TappedEventArgs e)
    {
        await OpenBrowserAsync(_textKeysUrl, forText: true);
    }

    private async void OnTextSaveClicked(object? sender, EventArgs e)
    {
        if (_savingText) return;

        var baseUrl = TextBaseUrlEntry.Text?.Trim() ?? "";
        var model = TextModelEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(model))
        {
            ShowTextStatus(AiSettingsService.T("SettingsInvalid"), isError: true);
            return;
        }

        var settings = new AiTextSettings(
            TextProviderToProvider(TextProviderPicker.SelectedIndex),
            TextApiKeyEntry.Text?.Trim() ?? "",
            baseUrl,
            model);

        _savingText = true;
        SetBusy(true);
        try
        {
            await _fairyTaleService.TestTextConnectionAsync(settings);
            await _settings.SaveTextAsync(settings);
            ShowTextStatus(AiSettingsService.T("SettingsTestOk"), isError: false);
        }
        catch (Exception ex)
        {
            ShowTextStatus(string.Format(AiSettingsService.T("SettingsTestFailed"), ex.Message), isError: true);
        }
        finally
        {
            _savingText = false;
            SetBusy(false);
        }
    }

    // ── Image provider ──
    private void OnImageProviderChanged(object? sender, EventArgs e)
    {
        (string linkKey, string? baseUrl, string? model, string keysUrl) = ImageProviderPicker.SelectedIndex switch
        {
            1 => ("ApiKeyLinkOpenAi", AiSettingsService.OpenAiBaseUrl, AiSettingsService.OpenAiImageModel, AiSettingsService.OpenAiKeysUrl),
            2 => ("ApiKeyLinkCustom", null, null, AiSettingsService.OpenAiKeysUrl),
            _ => ("ApiKeyLinkTogether", AiSettingsService.TogetherBaseUrl, AiSettingsService.TogetherModel, AiSettingsService.TogetherKeysUrl)
        };

        ImageApiKeyLinkLabel.Text = AiSettingsService.T(linkKey);
        _imageKeysUrl = keysUrl;

        if (baseUrl is not null) ImageBaseUrlEntry.Text = baseUrl;
        if (model is not null) ImageModelEntry.Text = model;
    }

    private async void OnImageApiKeyLinkTapped(object? sender, TappedEventArgs e)
    {
        await OpenBrowserAsync(_imageKeysUrl, forText: false);
    }

    private async void OnImageSaveClicked(object? sender, EventArgs e)
    {
        if (_savingImage) return;

        var baseUrl = ImageBaseUrlEntry.Text?.Trim() ?? "";
        var model = ImageModelEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(model))
        {
            ShowImageStatus(AiSettingsService.T("SettingsInvalid"), isError: true);
            return;
        }

        var settings = new AiImageSettings(
            ImageProviderToProvider(ImageProviderPicker.SelectedIndex),
            ImageApiKeyEntry.Text?.Trim() ?? "",
            baseUrl,
            model);

        _savingImage = true;
        SetBusy(true);
        try
        {
            await _imageService.TestImageConnectionAsync(settings);
            await _settings.SaveImageAsync(settings);
            ShowImageStatus(AiSettingsService.T("SettingsTestOk"), isError: false);
        }
        catch (Exception ex)
        {
            ShowImageStatus(string.Format(AiSettingsService.T("SettingsTestFailed"), ex.Message), isError: true);
        }
        finally
        {
            _savingImage = false;
            SetBusy(false);
        }
    }

    // ── Provider <-> index mapping ──
    private static int TextProviderToIndex(string provider) => provider switch
    {
        AiSettingsService.TextProviderOpenRouter => 1,
        AiSettingsService.TextProviderCustom => 2,
        _ => 0
    };

    private static string TextProviderToProvider(int index) => index switch
    {
        1 => AiSettingsService.TextProviderOpenRouter,
        2 => AiSettingsService.TextProviderCustom,
        _ => AiSettingsService.TextProviderDeepSeek
    };

    private static int ImageProviderToIndex(string provider) => provider switch
    {
        AiSettingsService.ImageProviderOpenAi => 1,
        AiSettingsService.ImageProviderCustom => 2,
        _ => 0
    };

    private static string ImageProviderToProvider(int index) => index switch
    {
        1 => AiSettingsService.ImageProviderOpenAi,
        2 => AiSettingsService.ImageProviderCustom,
        _ => AiSettingsService.ImageProviderTogether
    };

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsRunning = busy;
        TextSaveBtn.IsEnabled = !busy;
        ImageSaveBtn.IsEnabled = !busy;
    }

    private async Task OpenBrowserAsync(string? url, bool forText)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        try
        {
            await Browser.OpenAsync(url);
        }
        catch (Exception ex)
        {
            if (forText) ShowTextStatus(ex.Message, isError: true);
            else ShowImageStatus(ex.Message, isError: true);
        }
    }

    private void ShowTextStatus(string message, bool isError)
    {
        TextStatusLabel.Text = message;
        TextStatusLabel.TextColor = isError ? Colors.Red : Colors.Green;
        TextStatusLabel.IsVisible = true;
    }

    private void ShowImageStatus(string message, bool isError)
    {
        ImageStatusLabel.Text = message;
        ImageStatusLabel.TextColor = isError ? Colors.Red : Colors.Green;
        ImageStatusLabel.IsVisible = true;
    }
}
