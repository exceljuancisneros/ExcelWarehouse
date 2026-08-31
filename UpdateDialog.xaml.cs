using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PrintLabels;

public partial class UpdateDialog : ContentPage
{
    private readonly string _latestVersion;
    private readonly string _downloadUrl;

    public UpdateDialog(string latestVersion, string downloadUrl)
    {
        InitializeComponent();
        _latestVersion = latestVersion;
        _downloadUrl = downloadUrl;
        
        VersionLabel.Text = $"Current: {VersionHelper.GetCurrentVersion()} → Latest: {_latestVersion}";
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_downloadUrl))
        {
            try
            {
                // Open GitHub release page in browser
                await Browser.Default.OpenAsync(_downloadUrl);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Download Failed", $"Could not open download link: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Update", "Update download URL not available. Please download from GitHub.", "OK");
        }
    }

    private void OnLaterClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync(animated: true);
    }
}
