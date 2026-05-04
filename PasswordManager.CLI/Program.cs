using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.Core.Commands;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Password Manager CLI");
        Console.WriteLine("=====================\n");

        // ----------------------------
        // Config
        // ----------------------------
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var apiUrl = config["Api:BaseUrl"];

        // ----------------------------
        // CORE SETUP
        // ----------------------------
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(apiUrl)
        };

        var crypto = new CryptoService();
        var codec = new VaultFormatCodec();
        var store = new ApiVaultStore(httpClient);

        var vaultService = new VaultService(crypto, store, codec);
        var app = new VaultApplication(vaultService);

        var dispatcher = new CommandDispatcher(
            new AddEntryHandler(app),
            new DeleteEntryHandler(app),
            new GeneratePasswordHandler(app)
        );

        // ----------------------------
        // SESSION STATE
        // ----------------------------
        string? userId = null;
        bool isUnlocked = false;

        // ----------------------------
        // CLI LOOP
        // ----------------------------
        while (true)
        {
            Console.WriteLine("\nCommands:");
            Console.WriteLine("1 - Unlock vault");
            Console.WriteLine("2 - List entries");
            Console.WriteLine("3 - Add entry");
            Console.WriteLine("4 - Delete entry");
            Console.WriteLine("5 - Generate password");
            Console.WriteLine("0 - Exit");

            Console.Write("\n> ");
            var input = Console.ReadLine();

            switch (input)
            {
                // ----------------------------
                // UNLOCK (DIRECT APP)
                // ----------------------------
                case "1":
                    Console.Write("User ID: ");
                    userId = Console.ReadLine();

                    Console.Write("Master password: ");
                    var password = Console.ReadLine();

                    await app.LoadVaultAsync(userId!, password!);
                    isUnlocked = true;

                    Console.WriteLine("Vault unlocked");
                    break;

                // ----------------------------
                // LIST (DIRECT APP)
                // ----------------------------
                case "2":
                    EnsureUnlocked(isUnlocked);

                    var entries = app.GetEntries();

                    Console.WriteLine($"\nEntries ({entries.Count}):");

                    foreach (var e in entries)
                    {
                        Console.WriteLine($"{e.Site} | {e.Username} | {e.Password}");
                    }

                    break;

                // ----------------------------
                // ADD (COMMAND)
                // ----------------------------
                case "3":
                    EnsureUnlocked(isUnlocked);

                    Console.Write("Site: ");
                    var site = Console.ReadLine();

                    Console.Write("Username: ");
                    var username = Console.ReadLine();

                    Console.Write("Password (blank = generate): ");
                    var passInput = Console.ReadLine();

                    var finalPassword = string.IsNullOrWhiteSpace(passInput)
                        ? PasswordGenerator.Generate(12)
                        : passInput;

                    var addCmd = new AddEntryCommand(
                        userId!,
                        new PasswordEntry
                        {
                            Site = site,
                            Username = username,
                            Password = finalPassword
                        }
                    );

                    await dispatcher.DispatchAsync(addCmd);

                    Console.WriteLine("Entry added");
                    break;

                // ----------------------------
                // DELETE (COMMAND)
                // ----------------------------
                case "4":
                    EnsureUnlocked(isUnlocked);

                    Console.Write("Site to delete: ");
                    var deleteSite = Console.ReadLine();

                    var deleteCmd = new DeleteEntryCommand(
                        userId!,
                        new PasswordEntry
                        {
                            Site = deleteSite
                        }
                    );

                    await dispatcher.DispatchAsync(deleteCmd);

                    Console.WriteLine("Entry deleted");
                    break;

                // ----------------------------
                // GENERATE (COMMAND QUERY)
                // ----------------------------
                case "5":
                    var genQuery = new GeneratePasswordQuery(12);

                    var generated = await dispatcher.DispatchAsync<string>(genQuery);

                    Console.WriteLine($"Generated: {generated}");
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid option");
                    break;
            }
        }
    }

    static void EnsureUnlocked(bool unlocked)
    {
        if (!unlocked)
            throw new InvalidOperationException("Vault is not unlocked");
    }
}