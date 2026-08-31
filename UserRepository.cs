namespace PrintLabels;

public static class UserRepository
{
    private const string ApiUrl = "http://192.168.210.50:8080/api/auth/login";

    public static async Task<(bool Authenticated, string ErrorMessage)> AuthenticateAsync(string username, string password)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var requestBody = new { UserName = username.Trim(), Password = password.Trim() };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return (false, "API connection failed. Please check your network connection.");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(responseJson);

            if (result != null && result.success)
            {
                // Store the JWT token for future use
                Preferences.Set("jwt_token", result.token);
                Preferences.Set("logged_in_user", username.Trim());
                return (true, null);
            }

            return (false, result?.message ?? "Invalid username or password.");
        }
        catch (TaskCanceledException)
        {
            return (false, "API connection timed out. Please check your network connection.");
        }
        catch (Exception)
        {
            return (false, "Could not connect to the server. Please check your network connection.");
        }
    }

    public static string GetToken()
    {
        return Preferences.Get("jwt_token", string.Empty);
    }

    public static string GetUsername()
    {
        return Preferences.Get("logged_in_user", string.Empty);
    }

    public static void ClearToken()
    {
        Preferences.Remove("jwt_token");
        Preferences.Remove("logged_in_user");
    }

    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(GetToken());
    }

    private class AuthResponse
    {
        public bool success { get; set; }
        public string token { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }
}
