using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class App : Application
{
    private readonly AuthService _auth;
    private readonly UserSession _session;

    public App(AuthService auth, UserSession session)
    {
        InitializeComponent();
        _auth = auth;
        _session = session;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        _ = Task.Run(async () =>
        {
            var result = await _auth.GetSilentAccountAsync();

            if (result != null)
            {
                _session.UserId = result.Account.HomeAccountId.Identifier;
            }
            else
            {
                var login = await _auth.LoginAsync();
                _session.UserId = login.Account.HomeAccountId.Identifier;
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync($"//{nameof(UnlockPage)}");
            });
        });

        return window;
    }
}