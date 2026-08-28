using Microsoft.Data.SqlClient;

namespace PrintLabels;

public partial class MainPage : ContentPage
{
    private readonly string _username;
    
    // SQL Server connection
    private const string Server = "192.168.211.17";
    private const string Database = "WItemLocations";
    private const string Username = "Excel_Apps";
    private const string Password = "!Excel.2019@!";
    private const string ViewName = "Find_Label_Items";

    public MainPage(string username)
    {
        _username = username;
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

            // Query SQL Server
            var product = await GetItemFromDatabase(itemCode);
            
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
        catch (SqlException sqlEx)
        {
            ErrorLabel.Text = $"Database error: {sqlEx.Message}";
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

    private static async Task<ItemData?> GetItemFromDatabase(string itemCode)
    {
        var connectionString = $"Server=tcp:{Server};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=true;Encrypt=false;Connect Timeout=10;";
        
        return await Task.Run(() =>
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            
            // Query the view by ItemNumber (since ItemCode column doesn't exist)
            var query = $@"SELECT TOP 1 * FROM {ViewName} 
                           WHERE ItemNumber = @ItemCode OR ItemCodeDesc = @ItemCode";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ItemCode", itemCode);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new ItemData
                {
                    ItemNumber = reader["ItemNumber"]?.ToString() ?? "",
                    ItemCodeDesc = reader["ItemCodeDesc"]?.ToString() ?? "",
                    Facility = reader["Facility"]?.ToString() ?? "",
                    Warehouse = reader["Warehouse"]?.ToString() ?? "",
                    Aisle = reader["Aisle"]?.ToString() ?? "",
                    Column = reader["Column"]?.ToString() ?? "",
                    Level = reader["Level"]?.ToString() ?? "",
                    Arrow = reader["Arrow"]?.ToString() ?? "",
                    Spot = reader["Spot"]?.ToString() ?? "",
                    Comment = reader["Comment"]?.ToString() ?? "",
                    Ver1 = reader["Ver1"]?.ToString() ?? "",
                    Ver2 = reader["Ver2"]?.ToString() ?? "",
                    Ver3 = reader["Ver3"]?.ToString() ?? "",
                    Ver4 = reader["Ver4"]?.ToString() ?? "",
                    Ver5 = reader["Ver5"]?.ToString() ?? "",
                    Ver6 = reader["Ver6"]?.ToString() ?? "",
                    Ver7 = reader["Ver7"]?.ToString() ?? ""
                };
            }
            return null;
        });
    }
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
