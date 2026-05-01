using PasswordManager.Core.Commands;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using static System.Net.Mime.MediaTypeNames;

namespace PasswordManager.UI;

public partial class VaultPage : ContentPage
{
    private readonly VaultApplication _app;
    private readonly CommandDispatcher _dispatcher;

    public VaultPage(VaultApplication app, CommandDispatcher dispatcher)
    {
        InitializeComponent();
        _app = app;
        _dispatcher = dispatcher;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadEntries();
    }

    private void LoadEntries()
    {
        var entries = _app.GetEntries();
        EntriesList.ItemsSource = null;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            EntriesList.ItemsSource = _app.GetEntries();
        });

        EntriesList.InvalidateMeasure();

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

        await _dispatcher.DispatchAsync(new AddEntryCommand(entry));


        // clear inputs
        SiteEntry.Text = "";
        UsernameEntry.Text = "";
        PasswordEntry.Text = "";

        LoadEntries();
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {

        var password = await _dispatcher.DispatchAsync(new GeneratePasswordQuery(10));


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

        await _dispatcher.DispatchAsync(new DeleteEntryCommand(entry));

    }
}