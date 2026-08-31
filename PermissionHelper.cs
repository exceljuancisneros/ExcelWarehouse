using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace PrintLabels
{
    public static class PermissionHelper
    {
        public static async Task<bool> RequestAllPermissionsAsync()
        {
            var granted = true;
            
            // Camera permission
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                    granted = false;
            }
            
            // Bluetooth permissions (handled via manifest)
            // Note: MAUI Essentials doesn't have BluetoothConnect/BluetoothScan permissions in .NET 10
            // Bluetooth is configured in AndroidManifest.xml
            
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                    granted = false;
            }
            
            return granted;
        }

        public static async Task ShowPermissionExplanationAsync()
        {
            var result = await Application.Current.MainPage.DisplayAlert(
                "Permissions Required",
                "Excel Warehouse needs the following permissions to work properly:\n\n" +
                "• Camera - For barcode scanning\n" +
                "• Bluetooth - For connecting to printers\n" +
                "• Location - For Bluetooth scanning on Android 11 and below\n\n" +
                "Please grant these permissions in Settings > Apps > Excel Warehouse > Permissions",
                "OK",
                "Cancel");
        }
    }
}
