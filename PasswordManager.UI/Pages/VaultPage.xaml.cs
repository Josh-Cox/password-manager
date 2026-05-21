using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Queries;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.UI.Helpers;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public partial class VaultPage : ContentPage
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;
    private readonly AuthService _auth;

    private ObservableCollection<PasswordEntry> _entries = new();
    private bool _isBusy;

    public VaultPage(CommandDispatcher dispatcher, UserSession session, AuthService auth)
    {
        InitializeComponent();

        _dispatcher = dispatcher;
        _session = session;
        _auth = auth;
    }

    private string UserId => _session.UserId!;

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        EntriesList.IsEnabled = !busy;
        AddButton.IsEnabled = !busy;
        SettingsButton.IsEnabled = !busy;
    }

    protected override async void OnAppearing()
    {
        Opacity = 0;
        base.OnAppearing();
        await this.FadeTo(1, 200, Easing.CubicOut);
        await LoadEntries();
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
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task ShowToast(string message)
    {
#if ANDROID || IOS || MACCATALYST
        var toast = Toast.Make(message, ToastDuration.Short, 14);
        await toast.Show();
#else
        CopiedBannerLabel.Text = message;
        CopiedBanner.IsVisible = true;
        CopiedBanner.Opacity = 0;
        await CopiedBanner.FadeToAsync(1, 150);
        await Task.Delay(1200);
        await CopiedBanner.FadeToAsync(0, 300);
        CopiedBanner.IsVisible = false;
#endif
    }

    // <================ Button Events ================> //

    private async void OnSettingsClicked(object sender, EventArgs e) => await SideMenu.OpenAsync();

    private async void OnMenuLogoutClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;
        try
        {
            await _dispatcher.DispatchAsync(new ClearSessionCommand());
            await _auth.LogoutAsync();
            _session.UserId = null;
            await this.FadeTo(0, 120, Easing.CubicIn);
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}?toast=logout", animate: false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
        }
    }

    private async void OnMenuDeleteAccountClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        bool confirm = await DisplayAlert(
            "Delete Account",
            "This will permanently delete your vault and your account. This cannot be undone.",
            "Delete",
            "Cancel"
        );

        if (!confirm) return;

        SetBusy(true);
        try
        {
            await _dispatcher.DispatchAsync(new DeleteAccountCommand(UserId));
            await _dispatcher.DispatchAsync(new ClearSessionCommand());
            await _auth.LogoutAsync();
            _session.UserId = null;
            await this.FadeTo(0, 120, Easing.CubicIn);
            await ShowToast("Account Deleted");
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}", animate: false);
            
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete account error: {ex.Message}");
            await DisplayAlert("Error", "Failed to delete account. Please try again.", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnEntryTapped(object sender, TappedEventArgs e)
    {
        if (_isBusy) return;

        var entry = e.Parameter as PasswordEntry;
        if (entry == null)
            return;

        await Clipboard.Default.SetTextAsync(entry.Password);
        await ShowToast($"{entry.Site} password copied");
    }

    private async void OnAddEntryClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        await this.FadeTo(0, 120, Easing.CubicIn);
        await Shell.Current.GoToAsync(nameof(AddEntryPage), animate: false);
    }

    private async void OnEditEntryClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        var button = (ImageButton)sender;
        var entry = button.CommandParameter as PasswordEntry;
        if (entry == null)
            return;

        var route = $"{nameof(AddEntryPage)}?site={Uri.EscapeDataString(entry.Site)}";

        await this.FadeTo(0, 120, Easing.CubicIn);
        await Shell.Current.GoToAsync(route, animate: false);
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        var button = (ImageButton)sender;
        var entry = button.CommandParameter as PasswordEntry;
        if (entry == null)
            return;

        bool confirm = await DisplayAlert(
            "Delete Entry",
            $"Delete {entry.Site}?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
            return;

        SetBusy(true);
        try
        {
            await _dispatcher.DispatchAsync(new DeleteEntryCommand(UserId, entry));
            await LoadEntries();
        }
        finally
        {
            SetBusy(false);
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
