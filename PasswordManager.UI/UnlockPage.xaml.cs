using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class UnlockPage : ContentPage
{
    private readonly VaultApplication _app;
    private readonly UserSession _session;

    public UnlockPage(VaultApplication app, UserSession session)
    {
        InitializeComponent();
        _app = app;
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

            await _app.LoadVaultAsync(_session.UserId!, password);

            await Shell.Current.GoToAsync(nameof(VaultPage));
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }
}