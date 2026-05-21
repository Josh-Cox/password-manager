namespace PasswordManager.UI.Controls;

public partial class SideMenu : ContentView
{
    public event EventHandler? LogoutClicked;
    public event EventHandler? DeleteAccountClicked;

    public SideMenu()
    {
        InitializeComponent();
    }

    public async Task OpenAsync()
    {
        MenuPanel.Scale = 0;
        MenuPanel.Opacity = 0;
        IsVisible = true;
        await Task.WhenAll(
            MenuPanel.ScaleTo(1, 200, Easing.CubicOut),
            MenuPanel.FadeTo(1, 160)
        );
    }

    public async Task CloseAsync()
    {
        await Task.WhenAll(
            MenuPanel.ScaleTo(0, 150, Easing.CubicIn),
            MenuPanel.FadeTo(0, 120)
        );
        IsVisible = false;
    }

    private async void OnBackdropTapped(object sender, TappedEventArgs e) => await CloseAsync();

    private void OnPanelTapped(object sender, TappedEventArgs e) { }

    private async void OnLogoutClicked(object sender, TappedEventArgs e)
    {
        await CloseAsync();
        LogoutClicked?.Invoke(this, EventArgs.Empty);
    }

    private async void OnDeleteAccountClicked(object sender, TappedEventArgs e)
    {
        await CloseAsync();
        DeleteAccountClicked?.Invoke(this, EventArgs.Empty);
    }
}
