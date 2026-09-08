using System.Text;
using System.Text.Json;

namespace PrintLabels;

public static class UserRepository
{
    private const string TokenUrl = "http://192.168.211.17:8082/connect/token";
    private const string ClientId = "excel_warehouse_maui";

    public static async Task<(bool Authenticated, string ErrorMessage, UserPermissions? Permissions)> AuthenticateAsync(string username, string password)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username.Trim(),
                ["password"] = password.Trim(),
                ["client_id"] = ClientId,
                ["scope"] = "openid profile roles offline_access"
            };

            var response = await httpClient.PostAsync(TokenUrl, new FormUrlEncodedContent(form));
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<TokenErrorResponse>(body);
                return (false, error?.error_description ?? "Usuario o contraseña incorrectos.", null);
            }

            var token = JsonSerializer.Deserialize<TokenResponse>(body)
                ?? throw new InvalidOperationException("Respuesta de token vacía.");

            var permissions = ExtractPermissions(token.access_token);
            if (!permissions.CanAccess)
                return (false, "No tenés permiso para acceder a esta aplicación.", null);

            await SecureStorage.SetAsync("access_token", token.access_token);
            await SecureStorage.SetAsync("refresh_token", token.refresh_token ?? string.Empty);
            Preferences.Set("logged_in_user", username.Trim());
            Preferences.Set("user_permissions", JsonSerializer.Serialize(permissions));

            return (true, string.Empty, permissions);
        }
        catch (TaskCanceledException)
        {
            return (false, "Se agotó el tiempo de conexión con el servidor. Verificá tu red.", null);
        }
        catch (Exception)
        {
            return (false, "No se pudo conectar con el servidor. Verificá tu red.", null);
        }
    }

    // Los roles ya vienen en el access token (claim "role") — no hace falta una segunda llamada
    // a GetUserPermissions como antes, ExcelIDPManager los resuelve al emitir el token.
    private static UserPermissions ExtractPermissions(string accessToken)
    {
        var segments = accessToken.Split('.');
        var payload = segments[1].Replace('-', '+').Replace('_', '/');
        payload += (payload.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        using var doc = JsonDocument.Parse(json);

        var roles = new HashSet<string>();
        if (doc.RootElement.TryGetProperty("role", out var roleElement))
        {
            if (roleElement.ValueKind == JsonValueKind.Array)
                foreach (var r in roleElement.EnumerateArray()) roles.Add(r.GetString() ?? "");
            else if (roleElement.ValueKind == JsonValueKind.String)
                roles.Add(roleElement.GetString() ?? "");
        }

        return new UserPermissions
        {
            CanAccess = roles.Contains("AppAccess"),
            CanPrint = roles.Contains("PrintLabels")
        };
    }

    public static Task<string?> GetAccessTokenAsync() => SecureStorage.GetAsync("access_token");

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
        SecureStorage.Remove("access_token");
        SecureStorage.Remove("refresh_token");
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

    private class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string? refresh_token { get; set; }
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }

    private class TokenErrorResponse
    {
        public string? error { get; set; }
        public string? error_description { get; set; }
    }
}
