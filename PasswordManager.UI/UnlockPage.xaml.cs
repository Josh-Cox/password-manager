using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class UnlockPage : ContentPage
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;

    public UnlockPage(CommandDispatcher dispatcher, UserSession session)
    {
        InitializeComponent();
        _dispatcher = dispatcher;
        _session = session;
    }

    private async void OnUnlockClicked(object sender, EventArgs e)
    {
        try
        {
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
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }


}