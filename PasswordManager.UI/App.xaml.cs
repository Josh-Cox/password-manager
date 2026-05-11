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

        _ = InitializeAsync();

        return window;
    }

    private async Task InitializeAsync()
    {
        try
        {
            await Task.Delay(50);

            var result = await _auth.GetSilentAccountAsync();

            if (result != null)
            {
                _session.UserId = result.Account.HomeAccountId.Identifier;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//UnlockPage");
                });
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//LoginPage");
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}