using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.App;
using System.Collections.Generic;
using System.Diagnostics;

namespace PrintLabels.Services;

public class BluetoothScanner
{
    private readonly BluetoothManager _bluetoothManager;
    private readonly BluetoothAdapter _bluetoothAdapter;
    private readonly List<BluetoothDevice> _pairedDevices = new();

    public BluetoothScanner()
    {
        try
        {
            var activity = Platform.CurrentActivity;
            System.Diagnostics.Debug.WriteLine($"[BluetoothScanner] CurrentActivity: {activity?.ToString() ?? "null"}");
            
            var context = activity != null
                ? (Context)activity
                : Android.App.Application.Context;

            System.Diagnostics.Debug.WriteLine($"[BluetoothScanner] Context: {context?.ToString() ?? "null"}");

            if (context != null)
            {
                _bluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            }
            
            System.Diagnostics.Debug.WriteLine($"[BluetoothScanner] BluetoothManager: {_bluetoothManager?.ToString() ?? "null"}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BluetoothScanner] Constructor Exception: {ex.Message}");
            _bluetoothManager = null;
        }

        try
        {
            _bluetoothAdapter = _bluetoothManager?.Adapter;
            System.Diagnostics.Debug.WriteLine($"[BluetoothScanner] BluetoothAdapter: {_bluetoothAdapter?.ToString() ?? "null"}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BluetoothScanner] Adapter Init Exception: {ex.Message}");
            _bluetoothAdapter = null;
        }
    }

    public bool IsBluetoothAvailable => _bluetoothManager != null && _bluetoothAdapter != null;
    public bool IsBluetoothEnabled => _bluetoothAdapter != null && _bluetoothAdapter.IsEnabled;

    public async Task<List<BluetoothDevice>> ScanForDevicesAsync(
        Action<BluetoothDevice> onDeviceFound,
        Action<string> onStatusUpdate,
        Action onCompleted,
        int timeoutSeconds = 10)
    {
        _pairedDevices.Clear();

        try
        {
            if (_bluetoothManager == null)
            {
                onStatusUpdate("BluetoothManager not available");
                onCompleted?.Invoke();
                return _pairedDevices;
            }

            if (_bluetoothAdapter == null)
            {
                onStatusUpdate("BluetoothAdapter not available");
                onCompleted?.Invoke();
                return _pairedDevices;
            }

            if (!_bluetoothAdapter.IsEnabled)
            {
                onStatusUpdate("Bluetooth is turned off. Please enable Bluetooth in settings.");
                onCompleted?.Invoke();
                return _pairedDevices;
            }

            onStatusUpdate("Reading paired Bluetooth devices...");

            try
            {
                var bondedDevices = _bluetoothAdapter.BondedDevices;
                if (bondedDevices != null && bondedDevices.Count > 0)
                {
                    foreach (BluetoothDevice device in bondedDevices)
                    {
                        var name = device.Name ?? device.Address;
                        _pairedDevices.Add(device);
                        onDeviceFound(device);
                    }
                }
            }
            catch (Java.Lang.SecurityException)
            {
                onStatusUpdate("Bluetooth permission not granted. Please grant in Settings > Apps > Excel Warehouse > Permissions.");
            }
            catch (System.Exception ex)
            {
                onStatusUpdate($"Error reading bonded devices: {ex.GetType().Name}: {ex.Message}");
            }
        }
        catch (System.Exception ex)
        {
            onStatusUpdate($"Scanner init error: {ex.GetType().Name}: {ex.Message}");
        }

        onStatusUpdate($"Found {_pairedDevices.Count} paired device(s)");
        onCompleted?.Invoke();

        return _pairedDevices;
    }

    public void CancelScan() { }
}
