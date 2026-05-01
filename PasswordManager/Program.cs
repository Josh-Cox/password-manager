using PasswordManager;
using System.Text.Json;
using TextCopy;
using System.Linq;
using PasswordManager.Core.Services;
using PasswordManager.Core.Models;
using PasswordManager.Core.Commands;

// define services
var crypto = new CryptoService();
var client = new HttpClient();
var fileStore = new ApiVaultStore(client);
var codec = new VaultFormatCodec();
var vault = new VaultService(crypto, fileStore, codec);
var searchService = new SearchServices();


// master password prompt & validation
Console.WriteLine("Enter your master password: ");

string? masterPassword = ReadHiddenInput();

var app = new VaultApplication(vault);
VaultSession session;

try
{
    app.LoadVaultAsync(masterPassword ?? "");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}


// main communication loop
while (true)
{
    Console.WriteLine("==== Password Manager ====");
    Console.WriteLine("1. Generate Password");
    Console.WriteLine("2. Save Entry");
    Console.WriteLine("3. View Entries");
    Console.WriteLine("4. Search");
    Console.WriteLine("5. Exit");
    Console.Write("Choose an option: ");

    // read user input
    string? input = Console.ReadLine();

    if (input == "1")
    {
        int length = ReadNumber("Enter password length: ");
    }
    else if (input == "2")
    {
        Console.Write("Site: ");
        string? site = Console.ReadLine();

        Console.Write("Username: ");
        string? username = Console.ReadLine();

        Console.Write("Password (leave empty to generate): ");
        string? password = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Site and username cannot be empty.");
            continue;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            password = PasswordService.Generate(12);
            Console.WriteLine($"Generated password: {password}");
        }

        PasswordEntry entry = new PasswordEntry
        {
            Site = site ?? "",
            Username = username ?? "",
            Password = password ?? ""
        };

        //var dispatcher = new CommandDispatcher(app);
        //await dispatcher.DispatchAsync(new AddEntryCommand(entry));

        Console.WriteLine("Entry saved.");
    }
    else if (input == "3")
    {
        if (app.GetEntries().Count == 0)
        {
            Console.WriteLine("No entries saved.");
        }
        else
        {
            var entries = app.GetEntries();
            int index = 1;
            foreach (var entry in entries)
            {
                Console.WriteLine($"{index}. {entry.Site}  |  {entry.Username}");
                index++;
            }

            Console.Write("Select entry number (or press Enter to go back): ");
            string? selection = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(selection))
                continue;

            if (int.TryParse(selection, out int selectedIndex) &&
                selectedIndex > 0 &&
                selectedIndex <= app.GetEntries().Count)
            {
                var entry = app.GetEntry(selectedIndex - 1);

                ShowEntry(entry);
            }
        }
    }
    else if (input == "4")
    {
        while (true)
        {
            Console.Write("Search (or press Enter to go back): ");
            string? query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query))
                break;

            var results = searchService.Search(app.GetEntries(), query);

            if (results.Count == 0)
            {
                Console.WriteLine("No matches found.");
                continue;
            }

            int index = 1;
            foreach (var entry in results)
            {
                Console.WriteLine($"{index}. {entry.Site} ({entry.Username})");
                index++;
            }

            Console.Write("Select entry number (or Enter to search again): ");
            string? selection = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(selection))
                continue;

            if (int.TryParse(selection, out int selectedIndex) &&
                selectedIndex > 0 &&
                selectedIndex <= results.Count)
            {
                var entry = results[selectedIndex - 1];

                ShowEntry(entry);
            }
        }
    }
    else if (input == "5")
    {
        break;
    }
    else
    {
        Console.WriteLine("Invalid option.");
    }

    Console.WriteLine();
}

/// <summary>
/// Method <c>ReadNumber</c> prompts the user for input and validates that it is a positive integer, repeatedly asking until valid input is received.
/// </summary>
/// <param name="prompt">The message to display to the user when asking for input.</param>
int ReadNumber(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int result) && result > 0)
            return result;

        Console.WriteLine("Please enter a valid number.");
    }
}

/// <summary>
/// Method <c>RealHiddenInput</c> reads user input from the console without displaying it (useful for passwords).
/// </summary>
string ReadHiddenInput()
{
    // create stringBuilder object
    var password = new System.Text.StringBuilder();

    while (true)
    {
        // read the key pressed
        var key = Console.ReadKey(intercept: true);

        // if enter pressed then break
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }
        // if backspace pressed then remove last character
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Length--;
            Console.Write("\b \b");
        }
        // if not control character then add to password
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    }

    return password.ToString();
}

/// <summary>
/// Method <c>CopyToClipboard</c> copies the provided text to the system clipboard using the TextCopy library.
/// </summary>
/// <param name="text">The text to be copied to the clipboard.</param>
void CopyToClipboard(string text)
{
    ClipboardService.SetText(text);
}

/// <summary>
/// Method <c>ShowEntry</c> displays the details of a password entry, including options to show the password and copy it to the clipboard.
/// </summary>
/// <param name="entry">The password entry to be displayed.</param>
void ShowEntry(PasswordEntry entry)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine($"Site: {entry.Site}");
        Console.WriteLine($"Username: {entry.Username}");
        Console.WriteLine();

        Console.WriteLine("1. Show password");
        Console.WriteLine("2. Copy password");
        Console.WriteLine("3. Back");
        Console.Write("Choose an option: ");

        string? input = Console.ReadLine();

        if (input == "1")
        {
            Console.WriteLine($"Password: {entry.Password}");
        }
        else if (input == "2")
        {
            CopyToClipboard(entry.Password);
            Console.WriteLine("Copied to clipboard.");
        }
        else if (input == "3")
        {
            break;
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }
    }
}