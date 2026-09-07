using System.Text.Json;

namespace PrintLabels;

public static class UserRepository
{
    private const string ApiUrl = "http://192.168.211.17:8080/api/ExcelIDP/VerifyUserCredentials";

    public static async Task<(bool Authenticated, string ErrorMessage, UserPermissions? Permissions)> AuthenticateAsync(string username, string password)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var requestBody = new { UserName = username.Trim(), Password = password.Trim() };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(ApiUrl, content);

            // Debug: log raw response
            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[LOGIN] Response from {ApiUrl}: {responseText}");
            Console.WriteLine($"[LOGIN] Status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[LOGIN] Failed - HTTP {response.StatusCode}");
                return (false, "API connection failed. Please check your network connection.", null);
            }

            var result = JsonSerializer.Deserialize<AuthResponse>(responseText);

            Console.WriteLine($"[LOGIN] Deserialized - success: {result?.success}, count: {result?.count}");
            Console.WriteLine($"[LOGIN] Permissions count: {result?.permissions?.Count ?? 0}");

            if (result?.permissions != null)
            {
                foreach (var perm in result.permissions)
                {
                    Console.WriteLine($"[LOGIN] Permission: {perm.permissionName} = {perm.value}");
                }
            }

            if (result == null || !result.success)
            {
                Console.WriteLine("[LOGIN] Failed - success=false or null");
                return (false, "Invalid username or password.", null);
            }

            // Parse permissions
            var permissions = ParsePermissions(result.permissions);

            Console.WriteLine($"[LOGIN] Parsed - CanAccess: {permissions.CanAccess}, CanPrint: {permissions.CanPrint}");

            // Check if user has app access
            if (!permissions.CanAccess)
            {
                Console.WriteLine("[LOGIN] BLOCKED - No app access permission");
                return (false, "You do not have permission to access this application.", null);
            }

            Console.WriteLine("[LOGIN] SUCCESS");

            // Store auth data
            Preferences.Set("logged_in_user", username.Trim());
            Preferences.Set("user_permissions", JsonSerializer.Serialize(permissions));

            return (true, null, permissions);
        }
        catch (TaskCanceledException)
        {
            return (false, "API connection timed out. Please check your network connection.", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGIN] EXCEPTION: {ex.Message}");
            return (false, "Could not connect to the server. Please check your network connection.", null);
        }
    }

    private static UserPermissions ParsePermissions(List<Permission>? permissions)
    {
        var result = new UserPermissions();

        if (permissions == null || permissions.Count == 0)
        {
            Console.WriteLine("[PARSE] No permissions found - using defaults");
            return result;
        }

        foreach (var perm in permissions)
        {
            Console.WriteLine($"[PARSE] Checking: {perm.permissionName} = '{perm.value}'");
            
            switch (perm.permissionName)
            {
                case "ExcelWarehouse_UPAppAccess":
                    result.CanAccess = perm.value == "True";
                    Console.WriteLine($"[PARSE] Set CanAccess = {result.CanAccess}");
                    break;
                case "ExcelWarehouse_UPPrintLabels":
                    result.CanPrint = perm.value == "True";
                    Console.WriteLine($"[PARSE] Set CanPrint = {result.CanPrint}");
                    break;
                default:
                    Console.WriteLine($"[PARSE] Unknown permission: {perm.permissionName}");
                    break;
            }
        }

        return result;
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

    private class AuthResponse
    {
        public bool success { get; set; }
        public int count { get; set; }
        public List<Permission>? permissions { get; set; }
    }

    private class Permission
    {
        public string permissionName { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
