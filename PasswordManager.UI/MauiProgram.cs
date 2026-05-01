using Microsoft.Extensions.Logging;
using PasswordManager.Core.Commands;
using PasswordManager.Core.Services;

namespace PasswordManager.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>();

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri("https://password-manager-api-josh-btecb9fqacb8bje8.westeurope-01.azurewebsites.net/")
            });

            // Core services
            builder.Services.AddSingleton<CryptoService>();
            builder.Services.AddSingleton<VaultFormatCodec>();

            builder.Services.AddSingleton<IVaultStore, ApiVaultStore>();

            builder.Services.AddSingleton<VaultService>();
            builder.Services.AddSingleton<VaultApplication>();

            builder.Services.AddSingleton<AddEntryHandler>();
            builder.Services.AddSingleton<DeleteEntryHandler>();
            builder.Services.AddSingleton<GeneratePasswordHandler>();

            builder.Services.AddSingleton<CommandDispatcher>();

            return builder.Build();
        }
    }
}
