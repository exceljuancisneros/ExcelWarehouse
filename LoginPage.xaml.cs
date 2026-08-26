namespace PrintLabels;

public static class UserRepository
{
    private static readonly Dictionary<string, string> _users = new()
    {
        { "admin", "Admin@1234!" },
        { "warehouse", "Excel.2026" }
    };

    public static bool Authenticate(string username, string password)
    {
        if (_users.TryGetValue(username.Trim().ToLower(), out var expected))
            return expected == password;
        return false;
    }
}

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UsernameEntry.Focus();
    }

    private void OnUsernameCompleted(object sender, EventArgs e)
    {
        PasswordEntry.Focus();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username))
        {
            ErrorLabel.Text = "Please enter a username.";
            ErrorLabel.IsVisible = true;
            LoginButton.IsEnabled = true;
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Please enter a password.";
            ErrorLabel.IsVisible = true;
            LoginButton.IsEnabled = true;
            return;
        }

        await Task.Delay(300);

        if (UserRepository.Authenticate(username, password))
        {
            await Navigation.PushAsync(new MenuPage(username));
        }
        else
        {
            ErrorLabel.Text = "Invalid username or password.";
            ErrorLabel.IsVisible = true;
        }

        LoginButton.IsEnabled = true;
    }
}