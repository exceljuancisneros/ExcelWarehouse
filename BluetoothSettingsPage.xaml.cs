using PrintLabels.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Android.Bluetooth;

namespace PrintLabels;

public partial class BluetoothSettingsPage : ContentPage
{
    private readonly ObservableCollection<BluetoothDeviceWrapper> _availablePrinters;
    private BluetoothDeviceWrapper? _selectedPrinter;
    private BluetoothScanner? _scanner;
    private bool _isScanning = false;

    private readonly MainPage _mainPage;

    public BluetoothSettingsPage(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
        _availablePrinters = new ObservableCollection<BluetoothDeviceWrapper>();
        PrinterList.ItemsSource = _availablePrinters;

        // Load saved printer from preferences
        var savedAddress = Preferences.Get("SelectedPrinterAddress", "");
        if (!string.IsNullOrEmpty(savedAddress))
        {
            var savedName = Preferences.Get("SelectedPrinterName", "");
            _selectedPrinter = new BluetoothDeviceWrapper
            {
                Name = savedName,
                Address = savedAddress
            };
            SelectedPrinterLabel.Text = savedName;
            StatusLabel.Text = "&#xf00c;  Printer saved";
            StatusLabel.TextColor = Color.FromArgb("#3fb950");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PrinterList.SelectedItem = _selectedPrinter;
        
        // Update status based on saved printer
        if (_selectedPrinter != null)
        {
            StatusLabel.Text = $"&#xf00c;  Printer: {_selectedPrinter.Name}";
            StatusLabel.TextColor = Color.FromArgb("#3fb950");
        }
        else
        {
            StatusLabel.Text = "&#xf071;  Tap 'Scan' to find printers";
            StatusLabel.TextColor = Color.FromArgb("#8b949e");
        }
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        if (_isScanning)
        {
            return;
        }

        try
        {
            _isScanning = true;
            _availablePrinters.Clear();
            PrinterList.SelectedItem = null;

            // Initialize scanner
            _scanner = new BluetoothScanner();
            
            // Update UI
            StatusLabel.Text = "&#xf06e;  Scanning...";
            StatusLabel.TextColor = Color.FromArgb("#f0883e");

            if (!_scanner.IsBluetoothAvailable)
            {
                StatusLabel.Text = "&#xf071;  Bluetooth not available on this device";
                StatusLabel.TextColor = Color.FromArgb("#f85149");
                await DisplayAlertAsync("Bluetooth Error", "This device does not have Bluetooth capability.", "OK");
                _isScanning = false;
                return;
            }

            if (!_scanner.IsBluetoothEnabled)
            {
                StatusLabel.Text = "&#xf071;  Bluetooth is OFF";
                StatusLabel.TextColor = Color.FromArgb("#f85149");
                await DisplayAlertAsync("Bluetooth Required", "Please turn on Bluetooth in settings to scan for printers.", "OK");
                _isScanning = false;
                return;
            }

            // Request ALL required permissions for Android 12+
            var granted = await RequestBluetoothPermissionsAsync();
            if (!granted)
            {
                StatusLabel.Text = "&#xf071;  Permission denied";
                StatusLabel.TextColor = Color.FromArgb("#f85149");
                await DisplayAlertAsync("Permission Required", 
                    "BLUETOOTH CONNECT permission is required.\n\n" +
                    "Please grant it manually:\n" +
                    "1. Go to Settings > Apps > Excel Warehouse\n" +
                    "2. Tap Permissions\n" +
                    "3. Enable 'Nearby Devices (Connect)'\n" +
                    "4. Then tap Scan again", 
                    "OK");
                _isScanning = false;
                return;
            }

            // Perform scan
            var devices = await _scanner.ScanForDevicesAsync(
                // When a device is found
                (device) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var wrapper = new BluetoothDeviceWrapper
                        {
                            Name = device.Name ?? device.Address,
                            Address = device.Address,
                            Device = device
                        };
                        _availablePrinters.Add(wrapper);
                    });
                },
                // Status updates
                (status) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusLabel.Text = $"&#xf06e;  {status}";
                    });
                },
                // Scan completed
                () =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (_availablePrinters.Count > 0)
                        {
                            StatusLabel.Text = $"&#xf00c;  {_availablePrinters.Count} device(s) found";
                            StatusLabel.TextColor = Color.FromArgb("#3fb950");
                        }
                        else
                        {
                            StatusLabel.Text = "&#xf071;  No devices found. Make sure your printer is paired with this device and Bluetooth is ON.";
                            StatusLabel.TextColor = Color.FromArgb("#f0883e");
                        }
                    });
                }
            );
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"&#xf071;  Error: {ex.Message}";
            StatusLabel.TextColor = Color.FromArgb("#f85149");
            Debug.WriteLine($"Scan error: {ex}");
            await DisplayAlertAsync("Scan Error", $"Failed to scan devices: {ex.Message}", "OK");
        }
        finally
        {
            _isScanning = false;
        }
    }

    private async Task<bool> RequestBluetoothPermissionsAsync()
    {
        // Check BLUETOOTH_CONNECT permission via Android API
        var activity = Platform.CurrentActivity;
        if (activity != null)
        {
            try
            {
                var pm = activity.PackageManager;
                var status = pm.CheckPermission("android.permission.BLUETOOTH_CONNECT", "com.companyname.printlabels");
                if (status != (int)Android.Content.PM.Permission.Granted)
                {
                    return false; // Not granted - user must grant manually
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        // Request location permission (required for Bluetooth scanning on Android 11 and below)
        try
        {
            var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return locationStatus == PermissionStatus.Granted;
        }
        catch
        {
            return true;
        }
    }

    private async void OnPrinterSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is BluetoothDeviceWrapper selected)
        {
            _selectedPrinter = selected;
            SelectedPrinterLabel.Text = selected.Name;
            StatusLabel.Text = $"&#xf00c;  Selected: {selected.Name}";
            StatusLabel.TextColor = Color.FromArgb("#3fb950");
        }
    }

    private void OnDeviceItemTapped(object sender, EventArgs e)
    {
        var border = sender as Border;
        if (border?.BindingContext is BluetoothDeviceWrapper device)
        {
            // Deselect all other items
            foreach (var item in _availablePrinters)
            {
                item.IsSelected = false;
            }
            
            device.IsSelected = true;
            _selectedPrinter = device;
            SelectedPrinterLabel.Text = device.Name;
            StatusLabel.Text = $"&#xf00c;  Selected: {device.Name}";
            StatusLabel.TextColor = Color.FromArgb("#3fb950");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selectedPrinter != null)
        {
            // Save the selected printer
            Preferences.Set("SelectedPrinterAddress", _selectedPrinter.Address);
            Preferences.Set("SelectedPrinterName", _selectedPrinter.Name);
            SelectedPrinterLabel.Text = _selectedPrinter.Name;
            StatusLabel.Text = $"&#xf00c;  Printer: {_selectedPrinter.Name}";
            StatusLabel.TextColor = Color.FromArgb("#3fb950");
        }
        else
        {
            await DisplayAlertAsync("Error", "Please select a printer first.", "OK");
            return;
        }

        // Navigate back to ItemCode Search Page
        await Navigation.PopAsync(animated: true);
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}

// Wrapper class for BluetoothDevice that doesn't reference Android-specific types
public class BluetoothDeviceWrapper : BindableObject
{
    private bool _isSelected;
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public object? Device { get; set; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}
