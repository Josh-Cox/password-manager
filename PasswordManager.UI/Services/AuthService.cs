using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace PasswordManager.UI.Services;

public class AuthService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;
    private IAccount? _cachedAccount;

    public AuthService(string clientId, string tenantId, string scope)
    {
        var authority =
            $"https://{tenantId}.ciamlogin.com/{tenantId}/PasswordManagerSignIn";

        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(authority)
            .WithRedirectUri("http://localhost")
            .Build();

        _scopes = new[] { scope };

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
        var account = accounts.FirstOrDefault();

        if (account == null)
            return null;

        try
        {
            var result = await _app
                .AcquireTokenSilent(_scopes, account)
                .ExecuteAsync();

            return result;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

    public async Task<AuthenticationResult> LoginAsync()
    {
        var result = await _app
            .AcquireTokenInteractive(_scopes)
            .WithUseEmbeddedWebView(false)
            .WithPrompt(Prompt.SelectAccount)
            .ExecuteAsync();

        _cachedAccount = result.Account;

        return result;
    }

    public async Task LogoutAsync()
    {
        var accounts = await _app.GetAccountsAsync();

        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }
}