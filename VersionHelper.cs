using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrintLabels;

public static class VersionHelper
{
    private const string GitHubApiUrl = "https://api.github.com/repos/exceljuancisneros/ExcelWarehouse/releases/latest";
    private const string GitHubToken = "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";
    
    public static async Task<string> GetLatestVersionAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {GitHubToken}");
            
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
            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {GitHubToken}");
            
            var response = await httpClient.GetAsync(GitHubApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var release = JsonSerializer.Deserialize<GitHubRelease>(json, options);
                if (release?.Assets != null)
                {
                    foreach (var asset in release.Assets)
                    {
                        if (asset.Name.EndsWith(".apk"))
                        {
                            return asset.BrowserDownloadUrl;
                        }
                    }
                }
            }
        }
        catch
        {
            // If we can't check, return null to avoid blocking
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
        public string TagName { get; set; }
        public List<Asset> Assets { get; set; }
    }
    
    public class Asset
    {
        public string Name { get; set; }
        public string BrowserDownloadUrl { get; set; }
    }
}
