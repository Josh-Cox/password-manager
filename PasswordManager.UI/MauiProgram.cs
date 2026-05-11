using Microsoft.Extensions.Configuration;
using PasswordManager.Core.Com.Handlers;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        IConfiguration config;

        using var stream = FileSystem
            .OpenAppPackageFileAsync("appsettings.json")
            .GetAwaiter()
            .GetResult();

        config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(config["Api:BaseUrl"]!)
        });

        builder.Services.AddSingleton<AuthService>(_ =>
        {
            var auth = config.GetSection("Auth");

            return new AuthService(
                auth["ClientId"]!,
                auth["TenantId"]!,
                auth["Scope"]!
            );
        });

        builder.Services.AddSingleton<UserSession>();

        builder.Services.AddSingleton<CryptoService>();
        builder.Services.AddSingleton<VaultFormatCodec>();

        builder.Services.AddSingleton<IVaultStore, ApiVaultStore>();
        builder.Services.AddSingleton<VaultService>();
        builder.Services.AddSingleton<VaultApplication>();

        builder.Services.AddSingleton<AddEntryHandler>();
        builder.Services.AddSingleton<DeleteEntryHandler>();
        builder.Services.AddSingleton<GeneratePasswordHandler>();
        builder.Services.AddSingleton<GetEntriesHandler>();
        builder.Services.AddSingleton<LoadVaultHandler>();

        builder.Services.AddSingleton<CommandDispatcher>();

        return builder.Build();
    }
}