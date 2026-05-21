using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PasswordManager.UI.Helpers;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

[QueryProperty(nameof(ToastMessage), "toast")]
public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private bool _isSigningIn;

    private string? _toastMessage;

    public string? ToastMessage
    {
        get => _toastMessage;
        set => _toastMessage = value;
    }

    public LoginPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Opacity = 0;
        await this.FadeTo(1, 200, Easing.CubicOut);

        if (_toastMessage == "logout")
        {
            _toastMessage = null;
            await ShowToast("Logged out");
        }
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

                    // LoginAsync sets UserId internally — no need to touch it here.
                    await _auth.LoginAsync();

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

    private async Task ShowToast(string message)
    {
#if ANDROID || IOS || MACCATALYST
        var toast = Toast.Make(message, ToastDuration.Short, 14);
        await toast.Show();
#else
        CopiedBannerLabel.Text = message;
        CopiedBanner.IsVisible = true;
        CopiedBanner.Opacity = 0;
        await CopiedBanner.FadeToAsync(1, 150);
        await Task.Delay(1200);
        await CopiedBanner.FadeToAsync(0, 300);
        CopiedBanner.IsVisible = false;
#endif
    }
}
