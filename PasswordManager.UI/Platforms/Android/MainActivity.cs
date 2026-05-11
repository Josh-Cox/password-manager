using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Microsoft.Identity.Client;
using Microsoft.Maui.ApplicationModel;

namespace PasswordManager.UI;

[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize
                               | ConfigChanges.Orientation
                               | ConfigChanges.UiMode
                               | ConfigChanges.ScreenLayout
                               | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Platform.Init(this, savedInstanceState);

        Microsoft.Maui.ApplicationModel.Platform.ActivityStateChanged += (s, e) =>
        {
            CurrentActivity = e.Activity;
        };
    }

    public static Android.App.Activity? CurrentActivity { get; private set; }
}