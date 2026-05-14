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

    protected override async void OnAppearing()
    {
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
                SiteEntry.Text =
                    _editingEntry.Site;

                UsernameEntry.Text =
                    _editingEntry.Username;

                PasswordEntry.Text =
                    _editingEntry.Password;
            }
        }
    }

    // <================ Button Events ================> //
    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        var password =
            await _dispatcher.DispatchAsync(
                new GeneratePasswordQuery(16));

        PasswordEntry.Text = password;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var site = SiteEntry.Text?.Trim();
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(site) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            StatusLabel.Text =
                "All fields are required";

            return;
        }

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

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible =
            !_isPasswordVisible;

        PasswordEntry.IsPassword =
            !_isPasswordVisible;
    }
}