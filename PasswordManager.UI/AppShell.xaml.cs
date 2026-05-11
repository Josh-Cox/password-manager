namespace PasswordManager.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(VaultPage), typeof(VaultPage));
        Routing.RegisterRoute(nameof(AddEntryPage), typeof(AddEntryPage));
        Routing.RegisterRoute(nameof(UnlockPage), typeof(UnlockPage));
    }
}