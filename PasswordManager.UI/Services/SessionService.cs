using PasswordManager.UI.Helpers;

namespace PasswordManager.UI.Services;

public class SessionService
{
    private readonly AuthService _auth;
    private readonly UserSession _session;

    private bool _isLoggingOut;

    public SessionService(
        AuthService auth,
        UserSession session)
    {
        _auth = auth;
        _session = session;
    }

    public async Task LogoutAsync()
    {
        await AsyncOperationHelper.RunAsync(
            async () =>
            {
                await _auth.LogoutAsync();

                _session.UserId = null;

                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            },
            () => _isLoggingOut,
            busy => _isLoggingOut = busy);
    }
}