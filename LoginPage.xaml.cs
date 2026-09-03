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
        
        // Check for updates after the UI is ready
        _ = CheckForUpdatesAsync();
    }
    
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            await Task.Delay(1000); // Wait a second for the page to fully appear
            var latestVersion = await VersionHelper.GetLatestVersionAsync();
            
            if (VersionHelper.IsNewerVersion(latestVersion))
            {
                var downloadUrl = await VersionHelper.GetLatestDownloadUrlAsync();
                
                // Show update dialog
                var updateDialog = new UpdateDialog(latestVersion, downloadUrl ?? "");
                await Navigation.PushModalAsync(updateDialog);
            }
        }
        catch
        {
            // Don't let update check errors crash the app
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