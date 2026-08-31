namespace PrintLabels;

public partial class LoginPage : ContentPage
{
    private bool _isPasswordVisible = false;

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

    private void OnEyeIconTapped(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        EyeIcon.Text = _isPasswordVisible ? "\uf070" : "\uf06e";
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

        var (isAuthenticated, errorMessage) = await UserRepository.AuthenticateAsync(username, password);
        if (isAuthenticated)
        {
            await Navigation.PushAsync(new MenuPage(username));
        }
        else
        {
            ErrorLabel.Text = errorMessage ?? "Invalid username or password.";
            ErrorLabel.IsVisible = true;
        }

        LoginButton.IsEnabled = true;
    }
}