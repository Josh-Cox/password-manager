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

    protected override async void OnAppearing()
    {
        Opacity = 0;
        base.OnAppearing();
        await this.FadeTo(1, 200, Easing.CubicOut);
    }

    // <================ Button Events ================> //
    private async void OnSignInClicked(object sender, EventArgs e)
    {
        if (_isSigningIn)
            return;

        try
        {
            await AsyncOperationHelper.RunAsync(
                async () =>
                {
                    StatusLabel.Text = string.Empty;
                    StatusLabel.IsVisible = false;

                    await LoadingOverlay.FadeInAsync();

                    var result = await _auth.LoginAsync();

                    _session.UserId = UserIdentityHelper.GetStableUserId(result);

                    await this.FadeTo(0, 120, Easing.CubicIn);
                    await Shell.Current.GoToAsync($"//{nameof(UnlockPage)}", animate: false);
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
            StatusLabel.Text = "Unable to sign in.";
            StatusLabel.IsVisible = true;
        }
        finally
        {
            await LoadingOverlay.FadeOutAsync();
        }
    }
}
