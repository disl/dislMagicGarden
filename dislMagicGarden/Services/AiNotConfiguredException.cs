namespace dislMagicGarden.Services;

/// <summary>
/// Thrown when an AI call is attempted before a provider/API key was configured.
/// Callers use this to redirect the user to the settings page.
/// </summary>
public class AiNotConfiguredException : InvalidOperationException
{
    public AiNotConfiguredException(string message) : base(message) { }
}
