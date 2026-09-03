namespace PrintLabels;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        // Request permissions on first launch
        await RequestAllPermissionsAsync();
    }

    private async Task RequestAllPermissionsAsync()
    {
        try
        {
            // Check if this is first launch
            var isFirstLaunch = !Preferences.ContainsKey("hasLaunched");
            
            if (isFirstLaunch)
            {
                // Request all needed permissions
                var allGranted = await PermissionHelper.RequestAllPermissionsAsync();
                
                if (!allGranted)
                {
                    // Show permission explanation
                    await PermissionHelper.ShowPermissionExplanationAsync();
                }
                
                // Mark as launched
                Preferences.Set("hasLaunched", true);
            }
        }
        catch
        {
            // Don't let permission errors crash the app
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var latestVersion = await VersionHelper.GetLatestVersionAsync();
            
            if (VersionHelper.IsNewerVersion(latestVersion))
            {
                var downloadUrl = await VersionHelper.GetLatestDownloadUrlAsync();
                
                // Show update dialog
                var updateDialog = new UpdateDialog(latestVersion, downloadUrl ?? "");
                await App.Current.MainPage.Navigation.PushModalAsync(updateDialog);
            }
        }
        catch
        {
            // Don't let update check errors crash the app
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new LoginPage())
        {
            BarBackgroundColor = Color.FromArgb("#1a1a2e"),
            BarTextColor = Color.FromArgb("#ffffff")
        });
    }
}
