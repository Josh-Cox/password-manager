using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System.IdentityModel.Tokens.Jwt;

namespace PasswordManager.UI.Services;

public class AuthService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;
    private readonly UserSession _session;

    private IAccount? _cachedAccount;
    private string? _cachedAccessToken;
    private DateTimeOffset _cachedAccessTokenExpiresOn;

    public AuthService(string clientId, string tenantId, string scope, UserSession session)
    {
        var authority = $"https://{tenantId}.ciamlogin.com/{tenantId}/PasswordManagerSignIn";

#if ANDROID
        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(authority)
            .WithRedirectUri($"msal{clientId}://auth")
            .Build();
#endif
#if WINDOWS
        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(authority)
            .WithRedirectUri("http://localhost")
            .Build();

        ConfigureTokenCache();
#endif

        _scopes = new[] { scope };
        _session = session;
    }

#if WINDOWS
    private void ConfigureTokenCache()
    {
        var storageProperties =
            new StorageCreationPropertiesBuilder(
                "PasswordManagerTokenCache",
                FileSystem.CacheDirectory)
            .Build();

        var cacheHelper =
            MsalCacheHelper.CreateAsync(storageProperties)
                .GetAwaiter()
                .GetResult();

        cacheHelper.RegisterCache(_app.UserTokenCache);
    }
#endif

    // Attempts login from the MSAL token cache — no UI shown.
    // Returns null if the user needs to log in interactively.
    // Sets UserId on success.
    public async Task<AuthenticationResult?> GetSilentAccountAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        var account = _cachedAccount ?? accounts.FirstOrDefault();

        if (account == null)
            return null;

        try
        {
            var result = await _app
                .AcquireTokenSilent(_scopes, account)
                .ExecuteAsync();

            StoreResult(result);
            return result;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

    // Shows the Entra login page. Sets UserId on success.
    public async Task<AuthenticationResult> LoginAsync()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
#endif

        var builder = _app
            .AcquireTokenInteractive(_scopes)
            .WithUseEmbeddedWebView(false)
            .WithPrompt(Prompt.SelectAccount);

#if ANDROID
        builder = builder.WithParentActivityOrWindow(activity);
#endif

        var result = await builder.ExecuteAsync();
        StoreResult(result);
        return result;
    }

    // Returns a valid access token, refreshing silently if needed.
    public async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedAccessToken)
            && _cachedAccessTokenExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return _cachedAccessToken;
        }

        var result = await GetSilentAccountAsync();
        return result?.AccessToken;
    }

    // Clears all MSAL accounts and the cached token. Does NOT navigate.
    public async Task LogoutAsync()
    {
        var accounts = await _app.GetAccountsAsync();

        foreach (var account in accounts)
            await _app.RemoveAsync(account);

        _cachedAccount = null;
        _cachedAccessToken = null;
        _cachedAccessTokenExpiresOn = default;
    }

    // Caches the token result and writes UserId into the shared session.
    // This is the single place in the app that sets UserId.
    private void StoreResult(AuthenticationResult result)
    {
        _cachedAccount = result.Account;
        _cachedAccessToken = result.AccessToken;
        _cachedAccessTokenExpiresOn = result.ExpiresOn;
        _session.UserId = ExtractUserId(result);
    }

    private static string ExtractUserId(AuthenticationResult result)
    {
        var token = !string.IsNullOrWhiteSpace(result.AccessToken)
            ? result.AccessToken
            : result.IdToken;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
        if (!string.IsNullOrWhiteSpace(oid))
            return oid;

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (!string.IsNullOrWhiteSpace(sub))
            return sub;

        throw new InvalidOperationException("No stable user ID found in token.");
    }
}
