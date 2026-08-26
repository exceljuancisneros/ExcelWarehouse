using System.Diagnostics;
using Android.Bluetooth;
using Android.Content;
using Java.Util;

namespace PrintLabels.Services;

/// <summary>
/// Sends ZPL data to a Bluetooth Zebra printer via RFCOMM socket.
/// </summary>
public static class BtPrintService
{
    /// <summary>
    /// Standard UUID for SPP (Serial Port Profile) on Bluetooth printers.
    /// </summary>
    private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
    
    // Cache the BluetoothAdapter for reuse
    private static BluetoothAdapter? _cachedAdapter;
    private static Context? _cachedContext;

    /// <summary>
    /// Sends ZPL data to the printer using the saved MAC address.
    /// Returns true if print succeeded, false otherwise.
    /// </summary>
    public static async Task<bool> PrintAsync(string zpl, int copies = 1)
    {
        var savedAddress = Preferences.Get("SelectedPrinterAddress", "");
        var savedName = Preferences.Get("SelectedPrinterName", "");

        if (string.IsNullOrEmpty(savedAddress))
        {
            Debug.WriteLine("[BtPrintService] No printer saved. Go to Settings and select a printer.");
            return false;
        }

        Debug.WriteLine($"[BtPrintService] Printing to: {savedName} ({savedAddress})");
        Debug.WriteLine($"[BtPrintService] Copies: {copies}");

        for (int copy = 1; copy <= copies; copy++)
        {
            if (copy > 1)
                Debug.WriteLine($"[BtPrintService] Printing copy {copy}/{copies}");

            try
            {
                // Get BluetoothManager via modern API
                var context = GetApplicationContext();
                if (context == null)
                {
                    Debug.WriteLine("[BtPrintService] Context is null");
                    return false;
                }

                var bluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
                if (bluetoothManager == null)
                {
                    Debug.WriteLine("[BtPrintService] BluetoothManager is null");
                    return false;
                }

                var adapter = bluetoothManager.Adapter;
                if (adapter == null)
                {
                    Debug.WriteLine("[BtPrintService] BluetoothAdapter is null");
                    return false;
                }

                // Check if device is bonded (paired)
                var bondedDevices = adapter.BondedDevices;
                bool isBonded = false;
                if (bondedDevices != null)
                {
                    foreach (var bondedDevice in bondedDevices)
                    {
                        if (bondedDevice.Address == savedAddress)
                        {
                            isBonded = true;
                            Debug.WriteLine($"[BtPrintService] Device {bondedDevice.Name} is bonded");
                            break;
                        }
                    }
                }

                if (!isBonded)
                {
                    Debug.WriteLine($"[BtPrintService] Device {savedAddress} is NOT bonded/paired");
                    return false;
                }

                // Get remote device
                var remoteDevice = adapter.GetRemoteDevice(savedAddress);
                if (remoteDevice == null)
                {
                    Debug.WriteLine("[BtPrintService] Could not create remote device");
                    return false;
                }

                // Create RFCOMM socket with SPP UUID
                var socket = remoteDevice.CreateRfcommSocketToServiceRecord(SppUuid);
                if (socket == null)
                {
                    Debug.WriteLine("[BtPrintService] Could not create RFCOMM socket");
                    return false;
                }

                Debug.WriteLine("[BtPrintService] Attempting to connect to printer...");
                
                // Connect to the printer with timeout (5 seconds)
                bool connected = false;
                bool connectionError = false;
                string errorMsg = "";
                
                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(5000); // 5 second timeout
                
                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            socket.Connect();
                            connected = true;
                            Debug.WriteLine("[BtPrintService] Connection successful!");
                        }
                        catch (Java.IO.IOException ex)
                        {
                            connectionError = true;
                            errorMsg = ex.Message;
                            Debug.WriteLine($"[BtPrintService] Connection failed: {ex.Message}");
                        }
                    }, cts.Token);
                }
                catch (System.OperationCanceledException)
                {
                    connectionError = true;
                    errorMsg = "Connection timed out after 5 seconds";
                    Debug.WriteLine("[BtPrintService] Connection timed out");
                }
                catch (Exception ex)
                {
                    connectionError = true;
                    errorMsg = ex.Message;
                    Debug.WriteLine($"[BtPrintService] Connection error: {ex.Message}");
                }
                
                if (!connected)
                {
                    socket.Close();
                    throw new Exception($"Bluetooth connection failed: {errorMsg}. Make sure the printer is ON and nearby.");
                }

                // Get output stream and send ZPL
                Debug.WriteLine("[BtPrintService] Sending ZPL data...");
                using var outputStream = socket.OutputStream;
                var bytes = System.Text.Encoding.ASCII.GetBytes(zpl);
                await outputStream.WriteAsync(bytes, 0, bytes.Length);
                await outputStream.FlushAsync();

                Debug.WriteLine($"[BtPrintService] ZPL sent: {bytes.Length} bytes");

                // Give printer time to process
                await Task.Delay(1000);

                // Close socket
                socket.Close();
                Debug.WriteLine($"[BtPrintService] Copy {copy} sent successfully to {savedName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BtPrintService] Copy {copy} failed: {ex.Message}");
                if (copy == 1)
                {
                    throw; // Re-throw on first copy so we get the detailed error
                }
                return false;
            }
        }

        return true;
    }
    
    private static Context? GetApplicationContext()
    {
        try
        {
            var activity = global::Android.App.Application.Context;
            if (activity != null)
            {
                _cachedContext = activity;
            }
            else if (_cachedContext == null)
            {
                _cachedContext = Android.App.Application.Context;
            }
            return _cachedContext;
        }
        catch
        {
            return Android.App.Application.Context;
        }
    }
}
