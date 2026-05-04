using PasswordManager.Core.Commands;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class VaultPage : ContentPage
{
    private readonly VaultApplication _app;
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;

    public VaultPage(
        VaultApplication app,
        CommandDispatcher dispatcher,
        UserSession session)
    {
        InitializeComponent();
        _app = app;
        _dispatcher = dispatcher;
        _session = session;
    }

    private string UserId => _session.UserId!;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEntries();
    }

    private void LoadEntries()
    {
        var entries = _app.GetEntries();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            EntriesList.ItemsSource = entries;
        });

        DebugLabel.Text = $"Entries: {entries.Count}";
    }

    private async void OnAddEntryClicked(object sender, EventArgs e)
    {
        var site = SiteEntry.Text;
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(site) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var entry = new PasswordEntry
        {
            Site = site,
            Username = username,
            Password = password
        };

        await _dispatcher.DispatchAsync(
            new AddEntryCommand(UserId, entry)
        );

        SiteEntry.Text = "";
        UsernameEntry.Text = "";
        PasswordEntry.Text = "";

        LoadEntries();
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        var password = await _dispatcher.DispatchAsync(
            new GeneratePasswordQuery(10)
        );

        PasswordEntry.Text = password;
    }

    private async void OnEntrySelected(object sender, SelectionChangedEventArgs e)
    {
        var entry = e.CurrentSelection.FirstOrDefault() as PasswordEntry;

        if (entry == null)
            return;

        await Clipboard.Default.SetTextAsync(entry.Password);

        await DisplayAlertAsync("Copied to Clipboard", entry.Site, "OK");

        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnViewEntryClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var entry = (PasswordEntry)button.BindingContext;

        await DisplayAlertAsync(entry.Site, entry.Password, "OK");
    }

    private async void OnEntryDeleted(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var entry = (PasswordEntry)button.BindingContext;

        bool confirm = await DisplayAlertAsync(
            "Confirm Delete",
            $"Delete entry for {entry.Site}?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
            return;

        await _dispatcher.DispatchAsync(
            new DeleteEntryCommand(UserId, entry)
        );

        LoadEntries();
    }
}