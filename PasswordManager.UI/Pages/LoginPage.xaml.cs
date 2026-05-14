using PasswordManager.UI.Helpers;
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

    // <================ Button Events ================> //
    private async void OnSignInClicked(object sender, EventArgs e)
    {
        try
        {
            await AsyncOperationHelper.RunAsync(
                async () =>
                {
                    StatusLabel.Text = string.Empty;
                    StatusLabel.IsVisible = false;

                    await LoadingOverlay.FadeInAsync();

                    var result = await _auth.LoginAsync();

                    _session.UserId = result.Account?.HomeAccountId?.Identifier ?? string.Empty;

                    await Shell.Current.GoToAsync($"//{nameof(UnlockPage)}");
                },
                () => _isSigningIn,
                busy =>
                {
                    _isSigningIn = busy;
                    SignInButton.IsEnabled = !busy;
                }
            );
        }
        catch
        {
            //TODO: Logging
            StatusLabel.Text = "Unable to sign in.";
            StatusLabel.IsVisible = true;
        }
        finally
        {
            await LoadingOverlay.FadeOutAsync();
        }
    }
}
