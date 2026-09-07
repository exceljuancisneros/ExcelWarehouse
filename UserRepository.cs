using System.Text.Json;

namespace PrintLabels;

public static class UserRepository
{
    private const string VerifyUrl = "http://192.168.211.17:8080/api/ExcelIDP/VerifyUserCredentials";
    private const string PermissionsUrl = "http://192.168.211.17:8080/api/ExcelIDP/GetUserPermissions";

    public static async Task<(bool Authenticated, string ErrorMessage, UserPermissions? Permissions)> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Step 1: Verify credentials
            var verifyResult = await VerifyCredentialsAsync(username, password);
            
            if (!verifyResult.success)
            {
                return (false, verifyResult.message ?? "Invalid username or password.", null);
            }

            if (string.IsNullOrEmpty(verifyResult.userId))
            {
                return (false, "Invalid response from server.", null);
            }

            // Step 2: Get user permissions using userId
            var permissions = await GetUserPermissionsAsync(verifyResult.userId);

            // Check if user has app access
            if (permissions == null || !permissions.CanAccess)
            {
                return (false, "You do not have permission to access this application.", null);
            }

            // Store auth data
            Preferences.Set("logged_in_user", username.Trim());
            Preferences.Set("user_permissions", JsonSerializer.Serialize(permissions));

            return (true, null, permissions);
        }
        catch (TaskCanceledException)
        {
            return (false, "API connection timed out. Please check your network connection.", null);
        }
        catch (Exception)
        {
            return (false, "Could not connect to the server. Please check your network connection.", null);
        }
    }

    private static async Task<VerifyResult> VerifyCredentialsAsync(string username, string password)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        var requestBody = new { UserName = username.Trim(), Password = password.Trim() };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(VerifyUrl, content);

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<VerifyResponse>(responseJson);

        return new VerifyResult
        {
            success = result?.success == true,
            message = result?.message,
            userId = result != null ? result.userId.ToString() : null
        };
    }

    private static async Task<UserPermissions?> GetUserPermissionsAsync(string userId)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        var requestBody = new { userId = userId };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(PermissionsUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PermissionsResponse>(responseJson);

        if (result?.permissions == null || result.permissions.Count == 0)
            return null;

        var permissions = new UserPermissions();
        foreach (var perm in result.permissions)
        {
            switch (perm.permissionName)
            {
                case "ExcelWarehouse_UPAppAccess":
                    permissions.CanAccess = perm.value == "True";
                    break;
                case "ExcelWarehouse_UPPrintLabels":
                    permissions.CanPrint = perm.value == "True";
                    break;
            }
        }

        return permissions;
    }

    public static string GetUsername()
    {
        return Preferences.Get("logged_in_user", string.Empty);
    }

    public static UserPermissions? GetPermissions()
    {
        var json = Preferences.Get("user_permissions", string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            return JsonSerializer.Deserialize<UserPermissions>(json);
        }
        return null;
    }

    public static void ClearToken()
    {
        Preferences.Remove("logged_in_user");
        Preferences.Remove("user_permissions");
    }

    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(GetUsername());
    }

    public class UserPermissions
    {
        public bool CanAccess { get; set; }
        public bool CanPrint { get; set; }
    }

    private class VerifyResult
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public string? userId { get; set; }
    }

    private class VerifyResponse
    {
        public bool success { get; set; }
        public int userId { get; set; }
        public string userName { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }

    private class PermissionsResponse
    {
        public bool success { get; set; }
        public int count { get; set; }
        public List<Permission>? permissions { get; set; }
        public string message { get; set; } = string.Empty;
    }

    private class Permission
    {
        public string permissionName { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
