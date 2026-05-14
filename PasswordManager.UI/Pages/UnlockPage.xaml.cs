using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Services;
using PasswordManager.UI.Helpers;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class UnlockPage : ContentPage
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;
    private bool _isUnlocking;
    private readonly AuthService _auth;
    private readonly SessionService _sessionService;

    public UnlockPage(
        CommandDispatcher dispatcher,
        UserSession session,
        AuthService auth,
        SessionService sessionService
    )
    {
        InitializeComponent();
        _dispatcher = dispatcher;
        _session = session;
        _auth = auth;
        _sessionService = sessionService;
    }

    protected override void OnAppearing()
    {
        if (_isUnlocking)
            return;

        base.OnAppearing();

        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //    MasterPasswordEntry.Focus();
        //});

        StatusLabel.Text = string.Empty;
        MasterPasswordEntry.Text = string.Empty;
    }

    // <================ Button Events ================> //

    //TODO: Button event helper
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        if (_isUnlocking)
            return;

        await _sessionService.LogoutAsync();
    }

    //TODO: Button event helper
    private async void OnUnlockClicked(object sender, EventArgs e)
    {
        try
        {
            await AsyncOperationHelper.RunAsync(
                async () =>
                {
                    StatusLabel.Text = string.Empty;
                    StatusLabel.IsVisible = false;

                    var password = MasterPasswordEntry.Text;

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        StatusLabel.Text = "Enter master password";
                        StatusLabel.IsVisible = true;
                        return;
                    }

                    await LoadingOverlay.ShowAsync();

                    await Task.Delay(50);

                    try
                    {
                        //TODO: Reduce password being passed
                        await _dispatcher.DispatchAsync(
                            new LoadVaultCommand(_session.UserId!, password)
                        );
                    }
                    catch
                    {
                        StatusLabel.Text = "Invalid password";
                        StatusLabel.IsVisible = true;
                        return;
                    }

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.GoToAsync(nameof(VaultPage));
                    });
                },
                () => _isUnlocking,
                busy =>
                {
                    _isUnlocking = busy;
                    UnlockButton.IsEnabled = !busy;
                }
            );
        }
        catch (Exception ex)
        {
            //TODO: Setup Logging system
            System.Diagnostics.Debug.WriteLine($"Unlock failed: {ex.Message}");

            StatusLabel.Text = "Invalid password";
            StatusLabel.IsVisible = true;
        }
        finally
        {
            MasterPasswordEntry.Text = string.Empty;
            await LoadingOverlay.HideAsync();
        }
    }
}
