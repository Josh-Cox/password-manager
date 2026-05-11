using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class UnlockPage : ContentPage
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;
    private bool _isUnlocking;
    private readonly AuthService _auth;

    public UnlockPage(CommandDispatcher dispatcher, UserSession session, AuthService auth)
    {
        InitializeComponent();
        _dispatcher = dispatcher;
        _session = session;
        _auth = auth;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            await _auth.LogoutAsync();

            _session.UserId = null;

            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
        }
    }

    private async void OnUnlockClicked(object sender, EventArgs e)
    {
        if (_isUnlocking)
            return;

        try
        {
            _isUnlocking = true;

            UnlockButton.IsEnabled = false;

            var password = MasterPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(password))
            {
                StatusLabel.Text = "Enter master password";
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await _dispatcher.DispatchAsync(
                new LoadVaultCommand(_session.UserId!, password));

            await Shell.Current.GoToAsync(nameof(VaultPage));
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
        finally
        {
            _isUnlocking = false;

            UnlockButton.IsEnabled = true;

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        StatusLabel.Text = "";

        MasterPasswordEntry.Text = "";
    }


}