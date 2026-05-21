using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Services;
using PasswordManager.UI.Helpers;

namespace PasswordManager.UI.Services;

public class SessionService
{
    private readonly AuthService _auth;
    private readonly UserSession _session;
    private readonly CommandDispatcher _dispatcher;

    private bool _isLoggingOut;

    public SessionService(
        AuthService auth,
        UserSession session,
        CommandDispatcher dispatcher)
    {
        _auth = auth;
        _session = session;
        _dispatcher = dispatcher;
    }

    public async Task LogoutAsync()
    {
        await AsyncOperationHelper.RunAsync(
            async () =>
            {
                await _dispatcher.DispatchAsync(new ClearSessionCommand());

                await _auth.LogoutAsync();

                _session.UserId = null;

                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            },
            () => _isLoggingOut,
            busy => _isLoggingOut = busy);
    }
}
