using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace dislMagicGarden.Services;

/// <summary>
/// Connects the user's own OpenRouter account via OAuth PKCE (https://openrouter.ai/docs/use-cases/oauth-pkce).
/// The user logs in at openrouter.ai and the app receives an API key of that account,
/// so the user pays OpenRouter directly and no key has to be copied by hand.
/// Callback: a one-shot HTTP listener on localhost (documented by OpenRouter for local apps).
/// </summary>
public class OpenRouterAuthService
{
    private const string AuthUrl = "https://openrouter.ai/auth";
    private const string KeyExchangeUrl = "https://openrouter.ai/api/v1/auth/keys";
    private const string KeyLabel = "Whimsy Tales";
    private const string CallbackPath = "/callback";

    // Custom scheme only used to bring the app back to the front after the login (see WebAuthenticationCallbackActivity)
    public const string ReturnScheme = "whimsytales-auth";
    private static readonly string ReturnUrl = $"{ReturnScheme}://done";

    private static readonly TimeSpan LoginTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ExchangeTimeout = TimeSpan.FromSeconds(30);

    // Browser may report "back in app" slightly before the callback request was processed
    private static readonly TimeSpan CallbackGracePeriod = TimeSpan.FromSeconds(3);

    // Max. wait for the user to come back to the app before exchanging the code anyway
    private static readonly TimeSpan ReturnToAppTimeout = TimeSpan.FromSeconds(60);

    // Network may need a moment after the app returns to the foreground
    private const int ExchangeAttempts = 3;
    private static readonly TimeSpan ExchangeRetryDelay = TimeSpan.FromSeconds(2);

    private const int CodeVerifierByteLength = 32;
    private const int MaxRequestHeaderBytes = 16 * 1024;

    private static readonly HttpClient Http = new() { Timeout = ExchangeTimeout };

    /// <summary>
    /// Opens the OpenRouter login and returns the API key of the user's account.
    /// Throws <see cref="OperationCanceledException"/> if the user leaves the login without authorizing.
    /// </summary>
    public async Task<string> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var codeVerifier = CreateCodeVerifier();
        var codeChallenge = CreateCodeChallenge(codeVerifier);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var callbackUrl = $"http://localhost:{port}{CallbackPath}";
            var authUri = new Uri(
                $"{AuthUrl}?callback_url={Uri.EscapeDataString(callbackUrl)}" +
                $"&code_challenge={codeChallenge}&code_challenge_method=S256" +
                $"&key_label={Uri.EscapeDataString(KeyLabel)}");

            Debug.WriteLine($"[OpenRouterAuth] waiting for callback on {callbackUrl}");

            using var loginCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            loginCts.CancelAfter(LoginTimeout);

            var codeTask = WaitForCodeAsync(listener, loginCts.Token);
            var browserTask = ShowLoginAsync(authUri);

            if (await Task.WhenAny(codeTask, browserTask) == browserTask)
            {
                // User is back in the app: either after the login (callback arrives any moment) or cancelled
                if (await Task.WhenAny(codeTask, Task.Delay(CallbackGracePeriod, cancellationToken)) != codeTask)
                {
                    loginCts.Cancel();
                    throw new OperationCanceledException("OpenRouter login was cancelled.");
                }
            }

            var code = await codeTask;

            // Exchange only when the app is in the foreground again: some vendors (seen on Realme)
            // block network access of background apps while the browser is in front
            await Task.WhenAny(browserTask, Task.Delay(ReturnToAppTimeout, cancellationToken));

            return await ExchangeCodeWithRetryAsync(code, codeVerifier, cancellationToken);
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async Task ShowLoginAsync(Uri authUri)
    {
        try
        {
            // Opens a Custom Tab; completes when the page navigates to ReturnUrl or the user returns to the app
            await WebAuthenticator.Default.AuthenticateAsync(authUri, new Uri(ReturnUrl));
        }
        catch (TaskCanceledException)
        {
            // User returned to the app without the return link, handled by the caller
        }
    }

    /// <summary>
    /// Accepts connections on the loopback listener until the OpenRouter redirect with "?code=..." arrives.
    /// </summary>
    private static async Task<string> WaitForCodeAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        while (true)
        {
            using var client = await listener.AcceptTcpClientAsync(cancellationToken);
            await using var stream = client.GetStream();

            var target = await ReadRequestTargetAsync(stream, cancellationToken);
            var code = ParseCode(target);
            if (code == null)
            {
                // e.g. /favicon.ico
                await WriteResponseAsync(stream, "404 Not Found", string.Empty, cancellationToken);
                continue;
            }

            await WriteResponseAsync(stream, "200 OK", BuildReturnPage(), cancellationToken);
            return code;
        }
    }

    private static async Task<string> ReadRequestTargetAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[MaxRequestHeaderBytes];
        var length = 0;
        while (length < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken);
            if (read == 0)
                break;

            length += read;
            if (Encoding.ASCII.GetString(buffer, 0, length).Contains("\r\n\r\n"))
                break;
        }

        // Request line: "GET /callback?code=... HTTP/1.1"
        var requestLine = Encoding.ASCII.GetString(buffer, 0, length).Split("\r\n")[0];
        var parts = requestLine.Split(' ');
        return parts.Length >= 2 ? parts[1] : string.Empty;
    }

    private static string? ParseCode(string target)
    {
        if (!target.StartsWith(CallbackPath, StringComparison.Ordinal))
            return null;

        var queryStart = target.IndexOf('?');
        if (queryStart < 0)
            return null;

        var code = HttpUtility.ParseQueryString(target[(queryStart + 1)..])["code"];
        return string.IsNullOrWhiteSpace(code) ? null : code;
    }

    private static async Task WriteResponseAsync(NetworkStream stream, string status, string html, CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(html);
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {status}\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            "Connection: close\r\n\r\n");

        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Page shown in the browser after the login: jumps back to the app automatically,
    /// with a button as fallback (some browsers only open app links after a tap).
    /// </summary>
    private static string BuildReturnPage()
    {
        var text = WebUtility.HtmlEncode(AiSettingsService.T("OpenRouterReturnText"));
        var button = WebUtility.HtmlEncode(AiSettingsService.T("OpenRouterReturnButton"));

        return $$"""
            <!doctype html>
            <html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Whimsy Tales</title>
            <style>
              body { font-family: sans-serif; text-align: center; padding: 48px 16px; background: #FFE2F1; color: #4A4A4A; }
              a { display: inline-block; margin-top: 24px; padding: 14px 28px; border-radius: 24px;
                  background: #FF9ECD; color: #fff; text-decoration: none; font-size: 18px; }
            </style></head>
            <body>
              <h2>✨ Whimsy Tales</h2>
              <p>{{text}}</p>
              <a href="{{ReturnUrl}}">{{button}}</a>
              <script>setTimeout(function () { location.href = "{{ReturnUrl}}"; }, 300);</script>
            </body></html>
            """;
    }

    private static async Task<string> ExchangeCodeWithRetryAsync(string code, string codeVerifier, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await ExchangeCodeAsync(code, codeVerifier, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == null && attempt < ExchangeAttempts)
            {
                // Connection-level failure (no HTTP answer), e.g. network not yet available again
                Debug.WriteLine($"[OpenRouterAuth] key exchange attempt {attempt} failed: {ex.Message}");
                await Task.Delay(ExchangeRetryDelay, cancellationToken);
            }
        }
    }

    private static async Task<string> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.Serialize(new
        {
            code,
            code_verifier = codeVerifier,
            code_challenge_method = "S256"
        });

        using var response = await Http.PostAsync(
            KeyExchangeUrl, new StringContent(body, Encoding.UTF8, "application/json"), cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"OpenRouter key exchange failed ({(int)response.StatusCode}): {json}", null, response.StatusCode);

        using var document = JsonDocument.Parse(json);
        var key = document.RootElement.TryGetProperty("key", out var keyElement) ? keyElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(key))
            throw new HttpRequestException("OpenRouter key exchange returned no key.");

        return key;
    }

    private static string CreateCodeVerifier() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(CodeVerifierByteLength));

    private static string CreateCodeChallenge(string codeVerifier) =>
        Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
