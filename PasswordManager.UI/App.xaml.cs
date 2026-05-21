using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class App : Application
{
    private readonly AuthService _auth;

    public App(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
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

            // GetSilentAccountAsync sets UserId internally if successful.
            var result = await _auth.GetSilentAccountAsync();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var route = result != null ? "//UnlockPage" : "//LoginPage";
                await Shell.Current.GoToAsync(route);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
