using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PrintLabels
{
    public static class UpdateService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _downloadPath = string.Empty;
        private static string _apkName = string.Empty;

        public static async Task<bool> DownloadAndInstallAsync(string downloadUrl)
        {
            try
            {
                // Show downloading message
                var currentPage = Application.Current?.MainPage;
                await currentPage?.DisplayAlert("Downloading", "Downloading update...", "OK");

                // Generate local filename
                _apkName = $"ExcelWarehouse-{DateTime.Now:yyyyMMdd-HHmmss}.apk";
                
                // Use public downloads directory
                var downloadsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "Local", "Downloads");
                Directory.CreateDirectory(downloadsDir);
                _downloadPath = Path.Combine(downloadsDir, _apkName);

                // Download file
                var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                var buffer = new byte[8192];
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(_downloadPath, FileMode.Create, FileAccess.Write, FileShare.None);
                
                int bytesRead;
                long totalRead = 0;
                while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                }

                fileStream.Flush();
                fileStream.Close();

                // Show installing message
                await currentPage?.DisplayAlert("Installing", "Installing update...", "OK");

                // Install APK
                await InstallApkAsync(_downloadPath);
                return true;
            }
            catch (Exception ex)
            {
                var currentPage = Application.Current?.MainPage;
                await currentPage?.DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
                return false;
            }
        }

        private static async Task InstallApkAsync(string filePath)
        {
            // Use Android's package installer
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
            var uri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));
            intent.SetDataAndType(uri, "application/vnd.android.package-archive");
            intent.SetFlags(Android.Content.ActivityFlags.NewTask);
            intent.SetFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
            
            // Start activity
            Android.App.Application.Context.StartActivity(intent);
        }
    }
}
