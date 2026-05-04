using Microsoft.Extensions.Configuration;
using PasswordManager.Core.Commands;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;

namespace PasswordManager.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.UseMauiApp<App>();

            // MANUAL config builder (works reliably in MAUI)
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // API
            var apiUrl = config["Api:BaseUrl"];

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            });

            // Auth
            builder.Services.AddSingleton<AuthService>(sp =>
            {
                var auth = config.GetSection("Auth");

                return new AuthService(
                    clientId: auth["ClientId"],
                    tenantId: auth["TenantId"],
                    scope: auth["Scope"]
                );
            });

            // Session (NEW)
            builder.Services.AddSingleton<UserSession>();

            // Core
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