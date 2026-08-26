using Android.App;
using Android.Content.PM;

namespace PrintLabels;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : MauiAppCompatActivity
{
}
