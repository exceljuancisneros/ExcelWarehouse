namespace PrintLabels;

public partial class MenuPage : ContentPage
{
    private readonly string _username;
    private readonly UserRepository.UserPermissions? _permissions;

    public MenuPage(string username, UserRepository.UserPermissions? permissions)
    {
        _username = username;
        _permissions = permissions;
        InitializeComponent();
    }

    private async void OnPrintLabelsTapped(object sender, EventArgs e)
    {
        var mainPage = new MainPage(_username, _permissions);
        await Navigation.PushAsync(mainPage);
        await mainPage.FocusEntryAsync();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        UserRepository.ClearToken();
        await Navigation.PopToRootAsync();
    }
}
