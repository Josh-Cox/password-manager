using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Queries;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

[QueryProperty(nameof(EditSite), "site")]
public partial class AddEntryPage : ContentPage
{
    private readonly CommandDispatcher _dispatcher;
    private readonly UserSession _session;

    private bool _isPasswordVisible;
    private bool _isBusy;

    public string? EditSite { get; set; }

    private PasswordEntry? _editingEntry;

    public AddEntryPage(
        CommandDispatcher dispatcher,
        UserSession session)
    {
        InitializeComponent();

        _dispatcher = dispatcher;
        _session = session;
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        SaveButton.IsEnabled = !busy;
        GenerateButton.IsEnabled = !busy;
    }

    protected override async void OnAppearing()
    {
        Opacity = 0;
        base.OnAppearing();

        if (!string.IsNullOrWhiteSpace(EditSite))
        {
            var entries =
                await _dispatcher.DispatchAsync(
                    new GetEntriesQuery());

            _editingEntry =
                entries.FirstOrDefault(x =>
                    x.Site == EditSite);

            if (_editingEntry != null)
            {
                SiteEntry.Text = _editingEntry.Site;
                UsernameEntry.Text = _editingEntry.Username;
                PasswordEntry.Text = _editingEntry.Password;
            }
        }

        await this.FadeTo(1, 200, Easing.CubicOut);
    }

    // <================ Button Events ================> //

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        var password =
            await _dispatcher.DispatchAsync(
                new GeneratePasswordQuery(16));

        PasswordEntry.Text = password;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        var site = SiteEntry.Text?.Trim();
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text; // never trim passwords

        if (string.IsNullOrWhiteSpace(site))
        {
            StatusLabel.Text = "Site name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            StatusLabel.Text = "Username is required.";
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            StatusLabel.Text = "Password is required.";
            return;
        }

        if (site.Length > 256)
        {
            StatusLabel.Text = "Site name must be 256 characters or fewer.";
            return;
        }

        if (username.Length > 256)
        {
            StatusLabel.Text = "Username must be 256 characters or fewer.";
            return;
        }

        if (password.Length > 512)
        {
            StatusLabel.Text = "Password must be 512 characters or fewer.";
            return;
        }

        SetBusy(true);
        try
        {
            if (_editingEntry != null)
            {
                await _dispatcher.DispatchAsync(
                    new DeleteEntryCommand(
                        _session.UserId!,
                        _editingEntry));
            }

            var newEntry = new PasswordEntry
            {
                Site = site,
                Username = username,
                Password = password
            };

            await _dispatcher.DispatchAsync(new AddEntryCommand(_session.UserId!, newEntry));

            await this.FadeTo(0, 120, Easing.CubicIn);
            await Shell.Current.GoToAsync("..", animate: false);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        await this.FadeTo(0, 120, Easing.CubicIn);
        await Shell.Current.GoToAsync("..", animate: false);
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
    }
}
