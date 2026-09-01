using System.Text.Json;

namespace PrintLabels;

public partial class MainPage : ContentPage
{
    private readonly string _username;
    private readonly HttpClient _httpClient;
    
    // API Server connection
    private const string ApiUrl = "http://192.168.211.17:8080/api";

    public MainPage(string username)
    {
        _username = username;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        InitializeComponent();
        // Focus ItemCode when page appears
        Appearing += (s, e) => {
            ItemEntry.Focus();
        };
    }

    public async Task FocusEntryAsync()
    {
        await Task.Delay(100); // Wait for page to appear
        ItemEntry.Text = "";
        ItemEntry.Focus();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new BluetoothSettingsPage(this));
    }

    private async void OnItemCompleted(object sender, EventArgs e)
    {
        var itemCode = ItemEntry.Text?.Trim();
        if (string.IsNullOrEmpty(itemCode))
        {
            ErrorLabel.Text = "Please enter an item code.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            // Show loading
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            ErrorLabel.IsVisible = false;

            // Query the API
            var product = await GetItemFromApi(itemCode);
            
            if (product != null)
            {
                // Navigate to preview with the fetched data
                await Navigation.PushAsync(new LabelPreviewPage(
                    this,
                    product.ItemNumber,
                    product.ItemCodeDesc,
                    itemCode,
                    product.Facility,
                    product.Warehouse,
                    product.Aisle,
                    product.Column,
                    product.Level,
                    product.Spot,
                    product.Comment,
                    product.Ver1,
                    product.Ver2,
                    product.Ver3,
                    product.Ver4,
                    product.Ver5,
                    product.Ver6,
                    product.Ver7
                ));
            }
            else
            {
                ErrorLabel.Text = $"Item '{itemCode}' not found.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (TaskCanceledException)
        {
            ErrorLabel.Text = "API connection timed out. Please check your network connection.";
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Error: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async Task<ItemData?> GetItemFromApi(string itemCode)
    {
        try
        {
            var requestBody = new { ItemCode = itemCode };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiUrl}/item/search", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ItemApiResponse>(responseJson);

            if (result != null && result.success)
            {
                return new ItemData
                {
                    ItemNumber = result.itemNumber,
                    ItemCodeDesc = result.itemCodeDesc,
                    Facility = result.facility,
                    Warehouse = result.warehouse,
                    Aisle = result.aisle,
                    Column = result.column,
                    Level = result.level,
                    Arrow = result.arrow,
                    Spot = result.spot,
                    Comment = result.comment,
                    Ver1 = result.ver1,
                    Ver2 = result.ver2,
                    Ver3 = result.ver3,
                    Ver4 = result.ver4,
                    Ver5 = result.ver5,
                    Ver6 = result.ver6,
                    Ver7 = result.ver7
                };
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public class ItemApiResponse
{
    public bool success { get; set; }
    public string itemNumber { get; set; } = "";
    public string itemCodeDesc { get; set; } = "";
    public string facility { get; set; } = "";
    public string warehouse { get; set; } = "";
    public string aisle { get; set; } = "";
    public string column { get; set; } = "";
    public string level { get; set; } = "";
    public string arrow { get; set; } = "";
    public string spot { get; set; } = "";
    public string comment { get; set; } = "";
    public string ver1 { get; set; } = "";
    public string ver2 { get; set; } = "";
    public string ver3 { get; set; } = "";
    public string ver4 { get; set; } = "";
    public string ver5 { get; set; } = "";
    public string ver6 { get; set; } = "";
    public string ver7 { get; set; } = "";
}

public class ItemData
{
    public string ItemNumber { get; set; } = "";
    public string ItemCodeDesc { get; set; } = "";
    public string Facility { get; set; } = "";
    public string Warehouse { get; set; } = "";
    public string Aisle { get; set; } = "";
    public string Column { get; set; } = "";
    public string Level { get; set; } = "";
    public string Arrow { get; set; } = "";
    public string Spot { get; set; } = "";
    public string Comment { get; set; } = "";
    public string Ver1 { get; set; } = "";
    public string Ver2 { get; set; } = "";
    public string Ver3 { get; set; } = "";
    public string Ver4 { get; set; } = "";
    public string Ver5 { get; set; } = "";
    public string Ver6 { get; set; } = "";
    public string Ver7 { get; set; } = "";
}
