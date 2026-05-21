using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Queries;
using PasswordManager.Core.Services;
using PasswordManager.UI.Helpers;
using PasswordManager.UI.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace PasswordManager.UI;

public partial class UnlockPage : ContentPage, INotifyPropertyChanged
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;
    private readonly AuthService _auth;
    private readonly SessionService _sessionService;

    private bool _vaultExists;

    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(CanUnlock));
        }
    }

    public bool CanUnlock => !_isBusy;

    public UnlockPage(
        CommandDispatcher dispatcher,
        UserSession session,
        AuthService auth,
        SessionService sessionService)
    {
        InitializeComponent();
        _dispatcher = dispatcher;
        _session = session;
        _auth = auth;
        _sessionService = sessionService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        if (_isBusy)
            return;

        Opacity = 0;
        base.OnAppearing();

        StatusLabel.Text = string.Empty;
        StatusLabel.IsVisible = false;
        MasterPasswordEntry.Text = string.Empty;
        UnlockButton.IsEnabled = false;

        // Fade in immediately so the transition feels instant.
        // Check vault existence in parallel — UI updates when ready.
        await this.FadeTo(1, 200, Easing.CubicOut);

        try
        {
            _vaultExists = await _dispatcher.DispatchAsync(
                new VaultExistsQuery(_session.UserId!));

            if (_vaultExists)
            {
                TitleLabel.Text = "Unlock Vault";
                SubtitleLabel.Text = "Enter your master password";
                UnlockButton.Text = "Unlock";
            }
            else
            {
                TitleLabel.Text = "No Vault Found";
                SubtitleLabel.Text = "Set a master password to create your vault (min. 12 characters)";
                UnlockButton.Text = "Create";
            }

            UnlockButton.IsEnabled = true;
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine(ex);
            TitleLabel.Text = "Unable to Verify Vault";
            SubtitleLabel.Text = "Please sign out and back in.";
            UnlockButton.Text = "Unlock";
            UnlockButton.IsEnabled = false;
            StatusLabel.Text = ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                ? $"Unable to verify your account ({(int?)ex.StatusCode}). Please sign out and back in."
                : "Unable to check vault status.";
            StatusLabel.IsVisible = true;
        }
    }

    // <================ Button Events ================> //

    private async void OnSettingsClicked(object sender, EventArgs e) => await SideMenu.OpenAsync();

    private async void OnMenuLogoutClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;
        await _sessionService.LogoutAsync();
    }

    private async void OnMenuDeleteAccountClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        bool confirm = await DisplayAlert(
            "Delete Account",
            "This will permanently delete your vault and your account. This cannot be undone.",
            "Delete",
            "Cancel"
        );

        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _dispatcher.DispatchAsync(new DeleteAccountCommand(_session.UserId!));
            await _sessionService.LogoutAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete account error: {ex.Message}");
            await DisplayAlert("Error", "Failed to delete account. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private async void OnUnlockClicked(object sender, EventArgs e)
    {
        if (_isBusy)
            return;

        IsBusy = true;

        try
        {
            System.Diagnostics.Debug.WriteLine("UNLOCK PRESSED.");
            System.Diagnostics.Debug.WriteLine(_isBusy);

            StatusLabel.Text = string.Empty;
            StatusLabel.IsVisible = false;

            var password = MasterPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(password))
            {
                StatusLabel.Text = "Enter master password.";
                StatusLabel.IsVisible = true;
                return;
            }

            await LoadingOverlay.ShowAsync();

            if (_vaultExists)
            {
                await _dispatcher.DispatchAsync(
                    new LoadVaultCommand(_session.UserId!, password));
            }
            else
            {
                await _dispatcher.DispatchAsync(
                    new CreateVaultCommand(_session.UserId!, password));
            }

            await this.FadeTo(0, 120, Easing.CubicIn);
            await Shell.Current.GoToAsync(nameof(VaultPage), animate: false);
        }
        catch (InvalidMasterPasswordException)
        {
            await LoadingOverlay.HideAsync();
            StatusLabel.Text = "Invalid master password.";
            StatusLabel.IsVisible = true;
            return;
        }
        catch (ArgumentException ex)
        {
            await LoadingOverlay.HideAsync();
            StatusLabel.Text = ex.Message;
            StatusLabel.IsVisible = true;
            return;
        }
        catch (VaultNotFoundException)
        {
            // Vault disappeared between the existence check and the unlock attempt.
            _vaultExists = false;
            TitleLabel.Text = "No Vault Found";
            SubtitleLabel.Text = "Set a master password to create your vault (min. 12 characters)";
            UnlockButton.Text = "Create";
            await LoadingOverlay.HideAsync();
            StatusLabel.Text = "No vault found. Please create one.";
            StatusLabel.IsVisible = true;
            return;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            StatusLabel.Text = "Something went wrong.";
            StatusLabel.IsVisible = true;
        }
        finally
        {
            MasterPasswordEntry.Text = string.Empty;

            await LoadingOverlay.HideAsync();

            IsBusy = false;
        }
    }
}
