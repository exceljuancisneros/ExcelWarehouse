using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrintLabels;

public static class VersionHelper
{
    private const string GitHubApiUrl = "https://api.github.com/repos/exceljuancisneros/ExcelWarehouse/releases/latest";
    
    public static async Task<string> GetLatestVersionAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            var response = await httpClient.GetAsync(GitHubApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var release = JsonSerializer.Deserialize<GitHubRelease>(json, options);
                return release?.TagName?.Replace("v", "") ?? "0.0";
            }
        }
        catch
        {
            // If we can't check, return current version to avoid blocking
        }
        return GetCurrentVersion();
    }
    
    public static async Task<string> GetLatestDownloadUrlAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            var response = await httpClient.GetAsync(GitHubApiUrl);
            if (!response.IsSuccessStatusCode)
                return null;
                
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var release = JsonSerializer.Deserialize<GitHubRelease>(json, options);
            
            if (release?.Assets == null || release.Assets.Count == 0)
                return null;
                
            foreach (var asset in release.Assets)
            {
                // Debug: log asset names
                Console.WriteLine($"[VersionCheck] Asset: {asset.Name}");
                
                if (asset.Name.EndsWith(".apk"))
                {
                    Console.WriteLine($"[VersionCheck] Found APK: {asset.BrowserDownloadUrl}");
                    return asset.BrowserDownloadUrl;
                }
            }
            
            Console.WriteLine("[VersionCheck] No .apk asset found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VersionCheck] Error: {ex.Message}");
        }
        return null;
    }
    
    public static string GetCurrentVersion()
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
    
    public static bool IsNewerVersion(string latestVersion)
    {
        if (!Version.TryParse(latestVersion, out var latest))
            return false;
            
        if (!Version.TryParse(GetCurrentVersion(), out var current))
            return false;
            
        return latest > current;
    }
    
    public class GitHubRelease
    {
        [System.Text.Json.Serialization.JsonPropertyName("tag_name")]
        public string TagName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("assets")]
        public List<Asset> Assets { get; set; }
    }
    
    public class Asset
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
}
