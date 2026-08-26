namespace PrintLabels;

public partial class MenuPage : ContentPage
{
    private readonly string _username;

    public MenuPage(string username)
    {
        _username = username;
        InitializeComponent();
    }

    private async void OnPrintLabelsTapped(object sender, EventArgs e)
    {
        var mainPage = new MainPage(_username);
        await Navigation.PushAsync(mainPage);
        await mainPage.FocusEntryAsync();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}