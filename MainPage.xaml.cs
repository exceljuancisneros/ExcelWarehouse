using System.Collections.ObjectModel;
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
            var results = await GetItemsFromApi(itemCode);
            
            if (results == null || results.Count == 0)
            {
                ErrorLabel.Text = $"Item '{itemCode}' not found.";
                ErrorLabel.IsVisible = true;
            }
            else if (results.Count == 1)
            {
                // Only one result - navigate directly
                var product = results[0];
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
                // Multiple results - show selection modal
                await ShowItemSelectionModal(results, itemCode);
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

    private async Task ShowItemSelectionModal(ObservableCollection<ItemData> items, string originalItemCode)
    {
        var modal = new ItemSelectionModal(items);
        await Navigation.PushModalAsync(modal);

        var selectedItem = await modal.SelectedItemTask;

        if (selectedItem != null)
        {
            // Navigate to preview with the selected item
            await Navigation.PushAsync(new LabelPreviewPage(
                this,
                selectedItem.ItemNumber,
                selectedItem.ItemCodeDesc,
                originalItemCode,
                selectedItem.Facility,
                selectedItem.Warehouse,
                selectedItem.Aisle,
                selectedItem.Column,
                selectedItem.Level,
                selectedItem.Spot,
                selectedItem.Comment,
                selectedItem.Ver1,
                selectedItem.Ver2,
                selectedItem.Ver3,
                selectedItem.Ver4,
                selectedItem.Ver5,
                selectedItem.Ver6,
                selectedItem.Ver7
            ));
        }
    }

    private async Task<ObservableCollection<ItemData>?> GetItemsFromApi(string itemCode)
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
            var result = System.Text.Json.JsonSerializer.Deserialize<ItemsApiResponse>(responseJson);

            if (result != null && result.success && result.items != null && result.items.Count > 0)
            {
                var items = new ObservableCollection<ItemData>();
                foreach (var item in result.items)
                {
                    items.Add(new ItemData
                    {
                        ItemNumber = item.itemNumber,
                        ItemCodeDesc = item.itemCodeDesc,
                        Facility = item.facility,
                        Warehouse = item.warehouse,
                        Aisle = item.aisle,
                        Column = item.column,
                        Level = item.level,
                        Arrow = item.arrow,
                        Spot = item.spot,
                        Comment = item.comment,
                        Ver1 = item.ver1,
                        Ver2 = item.ver2,
                        Ver3 = item.ver3,
                        Ver4 = item.ver4,
                        Ver5 = item.ver5,
                        Ver6 = item.ver6,
                        Ver7 = item.ver7
                    });
                }
                return items;
            }

            return new ObservableCollection<ItemData>();
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public class ItemsApiResponse
{
    public bool success { get; set; }
    public int count { get; set; }
    public List<ItemApiResponse> items { get; set; } = new();
    public string message { get; set; } = "";
}

public class ItemApiResponse
{
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
