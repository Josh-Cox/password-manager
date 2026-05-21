namespace PasswordManager.UI.Controls;

public partial class AppHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AppHeader), string.Empty);

    public static readonly BindableProperty ShowBackButtonProperty =
        BindableProperty.Create(nameof(ShowBackButton), typeof(bool), typeof(AppHeader), false);

    public event EventHandler? BackClicked;
    public event EventHandler? MenuClicked;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    public AppHeader()
    {
        InitializeComponent();
    }

    private void OnBackClicked(object sender, EventArgs e) => BackClicked?.Invoke(this, e);
    private void OnMenuClicked(object sender, EventArgs e) => MenuClicked?.Invoke(this, e);
}
