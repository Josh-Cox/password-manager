using PasswordManager.Core.Services;

namespace PasswordManager.UI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        private readonly VaultApplication _app;

        public MainPage(VaultApplication app)
        {
            InitializeComponent();
            _app = app;
        }

        private async void OnUnlockClicked(Object sender, EventArgs e)
        {
            try
            {
                var password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(password))
                {
                    StatusLabel.Text = "Password required";
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Loading Vault...");

                await _app.LoadVaultAsync(password);

                StatusLabel.Text = "Vault unlocked";

                await Shell.Current.GoToAsync(nameof(VaultPage));
            }
            catch (Exception ex)
            {
                StatusLabel.Text = ex.Message;
            }
        }
    }
}
