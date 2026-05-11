using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly UserSession _session;
    private bool _isSigningIn;

    public LoginPage(AuthService auth, UserSession session)
    {
        InitializeComponent();
        _auth = auth;
        _session = session;
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        if (_isSigningIn)
            return;

        try
        {
            _isSigningIn = true;

            SignInButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var result = await _auth.LoginAsync();

            _session.UserId =
                result.Account.HomeAccountId.Identifier;

            await Shell.Current.GoToAsync($"//{nameof(UnlockPage)}");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
        finally
        {
            _isSigningIn = false;

            SignInButton.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}