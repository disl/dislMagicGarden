using Android.App;
using Android.Content;
using Android.Content.PM;
using dislMagicGarden.Services;

namespace dislMagicGarden.Platforms.Android
{
    /// <summary>
    /// Receives the return link after the OpenRouter login (see OpenRouterAuthService.ReturnScheme)
    /// and closes the Custom Tab so the user is back in the app.
    /// </summary>
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = OpenRouterAuthService.ReturnScheme)]
    public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}
