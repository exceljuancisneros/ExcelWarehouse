namespace PrintLabels;

public partial class LoginPage : ContentPage
{
    private bool _isPasswordVisible = false;

    public LoginPage()
    {
        InitializeComponent();
        VersionLabel.Text = "Version " + GetCurrentVersion();
    }

    private static string GetCurrentVersion()
    {
#if ANDROID
        try
        {
            var packageInfo = Android.App.Application.Context.PackageManager.GetPackageInfo(
                Android.App.Application.Context.PackageName, 0);
            return packageInfo.VersionName ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
#else
        return "1.0.0";
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UsernameEntry.Focus();
        
        // Check for updates
        DebugLabel.Text = "Checking...";
        DebugLabel.IsVisible = true;
        
        try
        {
            var latestVersion = await VersionHelper.GetLatestVersionAsync();
            DebugLabel.Text = $"Latest: {latestVersion} | Current: {VersionHelper.GetCurrentVersion()}";
            
            var isNewer = VersionHelper.IsNewerVersion(latestVersion);
            DebugLabel.Text = $"IsNewer: {isNewer} | Latest: {latestVersion}";
            
            if (isNewer)
            {
                var downloadUrl = await VersionHelper.GetLatestDownloadUrlAsync();
                
                await Task.Delay(1000);
                
                bool update = await DisplayAlert(
                    "Update Available",
                    $"A new version ({latestVersion}) is available.\n\nCurrent: {VersionHelper.GetCurrentVersion()}\nLatest: {latestVersion}",
                    "Download",
                    "Later");
                
                if (update && !string.IsNullOrEmpty(downloadUrl))
                {
                    DebugLabel.Text = "Button clicked! Opening browser...";
                    DebugLabel.IsVisible = true;
                    
                    // Open the GitHub release page in browser
                    var releaseUrl = "https://github.com/exceljuancisneros/ExcelWarehouse/releases/latest";
#if ANDROID
                    DebugLabel.Text = "Starting Android Intent...";
                    try
                    {
                        var uri = Android.Net.Uri.Parse(releaseUrl);
                        var intent = new Android.Content.Intent(Android.Content.Intent.ActionView, uri);
                        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                        if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity != null)
                        {
                            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.StartActivity(intent);
                            DebugLabel.Text = "Intent started!";
                        }
                        else
                        {
                            DebugLabel.Text = "CurrentActivity is NULL";
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLabel.Text = $"Intent error: {ex.Message}";
                    }
#else
                    await Browser.Default.OpenAsync(releaseUrl, BrowserLaunchMode.External);
                    DebugLabel.Text = "Opening release page...";
#endif
                }
            }
            else
            {
                DebugLabel.Text = "No update needed";
            }
        }
        catch (Exception ex)
        {
            DebugLabel.Text = $"ERROR: {ex.Message}";
        }
    }

    private void OnUsernameCompleted(object sender, EventArgs e)
    {
        PasswordEntry.Focus();
    }

    private void OnEyeIconTapped(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        EyeIcon.Text = _isPasswordVisible ? "\uf070" : "\uf06e";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username))
        {
            ErrorLabel.Text = "Please enter a username.";
            ErrorLabel.IsVisible = true;
            LoginButton.IsEnabled = true;
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Please enter a password.";
            ErrorLabel.IsVisible = true;
            LoginButton.IsEnabled = true;
            return;
        }

        await Task.Delay(300);

        var (isAuthenticated, errorMessage) = await UserRepository.AuthenticateAsync(username, password);
        if (isAuthenticated)
        {
            await Navigation.PushAsync(new MenuPage(username));
        }
        else
        {
            ErrorLabel.Text = errorMessage ?? "Invalid username or password.";
            ErrorLabel.IsVisible = true;
        }

        LoginButton.IsEnabled = true;
    }
}