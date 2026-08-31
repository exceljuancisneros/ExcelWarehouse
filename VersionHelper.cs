using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

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
        return "0.5"; // Update this manually when releasing new versions
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
