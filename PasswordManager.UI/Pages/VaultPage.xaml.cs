using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Queries;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class VaultPage : ContentPage
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;
    private readonly AuthService _auth;

    private ObservableCollection<PasswordEntry> _entries = new();

    public VaultPage(CommandDispatcher dispatcher, UserSession session, AuthService auth)
    {
        InitializeComponent();

        _dispatcher = dispatcher;
        _session = session;
        _auth = auth;
    }

    private string UserId => _session.UserId!;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            //DebugLabel.Text = $"User: {_session.UserId}";

            await LoadEntries();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("VaultPage Error", ex.ToString(), "OK");
        }
    }

    //TODO: Secure toast pop-ups
    //TODO: Helpers class
    private async Task ShowToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short, 14);

        await toast.Show();
    }

    private async Task LoadEntries()
    {
        try
        {
            var entries = await _dispatcher.DispatchAsync(new GetEntriesQuery());

            _entries = new ObservableCollection<PasswordEntry>(entries);

            EntriesList.ItemsSource = _entries;
        }
        catch (Exception ex)
        {
            //TODO: Setup Logging system
            await DisplayAlertAsync("LoadEntries Error", ex.ToString(), "OK");
        }
    }

    //TODO: Create central events dispatcher

    // <================ Button Events ================> //
    private async void OnEntryTapped(object sender, TappedEventArgs e)
    {
        var entry = e.Parameter as PasswordEntry;
        if (entry == null)
            return;

        await Clipboard.Default.SetTextAsync(entry.Password);

        await ShowToast($"{entry.Site} password copied");
    }

    private async void OnAddEntryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddEntryPage));
    }

    private async void OnCopyClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        var entry = (PasswordEntry)button.BindingContext;

        await Clipboard.Default.SetTextAsync(entry.Password);

        await DisplayAlertAsync("Copied", $"{entry.Site} password copied", "OK");
    }

    private async void OnEditEntryClicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;

        var entry = button.CommandParameter as PasswordEntry;
        if (entry == null)
            return;

        var route = $"{nameof(AddEntryPage)}?site={Uri.EscapeDataString(entry.Site)}";

        await Shell.Current.GoToAsync(route);
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;

        var entry = button.CommandParameter as PasswordEntry;
        if (entry == null)
            return;

        //TODO: Logging
        bool confirm = await DisplayAlertAsync(
            "Delete Entry",
            $"Delete {entry.Site}?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
            return;

        await _dispatcher.DispatchAsync(new DeleteEntryCommand(UserId, entry));

        await LoadEntries();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            await _auth.LogoutAsync();

            _session.UserId = null;

            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
        }
    }

    // <================ Search Events ================> //
    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var search = e.NewTextValue?.ToLower() ?? "";

        var filtered = _entries
            .Where(x =>
                (x.Site?.ToLower().Contains(search) ?? false)
                || (x.Username?.ToLower().Contains(search) ?? false)
            )
            .ToList();

        EntriesList.ItemsSource = filtered;
    }
}
