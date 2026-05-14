using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Handlers;
using PasswordManager.Core.Com.Handlers;
using PasswordManager.Core.Services;
using PasswordManager.UI.Services;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Platform;

#if ANDROID
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using AndroidX.AppCompat.Widget;
#endif

namespace PasswordManager.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        IConfiguration config;

        using var stream = FileSystem
            .OpenAppPackageFileAsync("appsettings.json")
            .GetAwaiter()
            .GetResult();

        config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Services.AddSingleton<UserSession>();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(config["Api:BaseUrl"]!)
        });

        builder.Services.AddSingleton<AuthService>(sp =>
        {
            var auth = config.GetSection("Auth");

            var session =
                sp.GetRequiredService<UserSession>();

            return new AuthService(
                auth["ClientId"]!,
                auth["TenantId"]!,
                auth["Scope"]!,
                session
            );
        });



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
        builder.Services.AddSingleton<SessionService>();

        builder.Services.AddSingleton<CommandDispatcher>();

    SearchBarHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, view) =>
    {
    #if ANDROID
        var searchView = handler.PlatformView;

        var linearLayout = searchView.GetChildAt(0) as Android.Widget.LinearLayout;
        linearLayout = linearLayout?.GetChildAt(2) as Android.Widget.LinearLayout;
        linearLayout = linearLayout?.GetChildAt(1) as Android.Widget.LinearLayout;

        if (linearLayout != null)
        {
            linearLayout.Background = null;
        }
    #endif
    });

        EntryHandler.Mapper.AppendToMapping("CustomEntryColors", (handler, view) =>
        {
#if ANDROID
            var editText = handler.PlatformView;

            var accent = Android.Graphics.Color.ParseColor("#709255");

            // Cursor color
            try
            {

                var cursorDrawable = editText.TextCursorDrawable;

                if (cursorDrawable != null)
                {
                    cursorDrawable.SetTint(accent);
                }
            }
            catch
            {
            }
#endif
        });


        return builder.Build();
    }
}