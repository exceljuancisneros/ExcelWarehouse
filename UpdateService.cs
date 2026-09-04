using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Android.App;

namespace PrintLabels
{
    public static class UpdateService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<bool> DownloadAndInstallAsync(string downloadUrl, Page currentPage)
        {
            try
            {
                // Show downloading message
                await currentPage.DisplayAlert("Downloading", "Downloading update...", "OK");

                // Generate local filename
                var apkName = $"ExcelWarehouse-{DateTime.Now:yyyyMMdd-HHmmss}.apk";
                
                // Use public downloads directory
                var downloadsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "Local", "Downloads");
                Directory.CreateDirectory(downloadsDir);
                var downloadPath = Path.Combine(downloadsDir, apkName);

                // Download file
                var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var buffer = new byte[8192];
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None);
                
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }

                fileStream.Flush();
                fileStream.Close();

                // Show installing message
                await currentPage.DisplayAlert("Installing", "Installing update...", "OK");

                // Install APK
                InstallApk(downloadPath);
                return true;
            }
            catch (Exception ex)
            {
                await currentPage.DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
                return false;
            }
        }

        private static void InstallApk(string filePath)
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
