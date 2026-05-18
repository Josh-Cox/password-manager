using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace PasswordManager.UI.Services;

public class AuthService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;
    private IAccount? _cachedAccount;
    private string? _cachedAccessToken;
    private DateTimeOffset _cachedAccessTokenExpiresOn;
    private readonly UserSession _session;

    public AuthService(
    string clientId,
    string tenantId,
    string scope,
    UserSession session)
    {
        var authority =
            $"https://{tenantId}.ciamlogin.com/{tenantId}/PasswordManagerSignIn";

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
#endif

        _scopes = new[] { scope };
        _session = session;

#if WINDOWS
        ConfigureTokenCache();
#endif
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

            _cachedAccount = result.Account;
            _cachedAccessToken = result.AccessToken;
            _cachedAccessTokenExpiresOn = result.ExpiresOn;
            _session.UserId = UserIdentityHelper.GetStableUserId(result);

            return result;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

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

        _cachedAccount = result.Account;
        _cachedAccessToken = result.AccessToken;
        _cachedAccessTokenExpiresOn = result.ExpiresOn;

        _session.UserId =
            UserIdentityHelper.GetStableUserId(result);

        System.Diagnostics.Debug.WriteLine(
    $"STABLE USER ID: {_session.UserId}");

        return result;
    }

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

    public async Task LogoutAsync()
    {
        var accounts = await _app.GetAccountsAsync();

        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }

        _cachedAccount = null;
        _cachedAccessToken = null;
        _cachedAccessTokenExpiresOn = default;
    }
}
