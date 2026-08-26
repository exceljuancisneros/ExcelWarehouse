using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PrintLabels.Services;

/// <summary>
/// Sends ZPL data to a Zebra printer over TCP/IP (Ethernet/Wi-Fi).
/// </summary>
public static class TcpPrintService
{
    private const int DefaultPort = 9100;

    /// <summary>
    /// Sends ZPL data to the printer at the given IP address and port.
    /// Returns true if print succeeded, false otherwise.
    /// </summary>
    public static async Task<bool> PrintAsync(string ip, int port = DefaultPort, string zpl = "", int copies = 1)
    {
        if (string.IsNullOrEmpty(ip))
        {
            Debug.WriteLine("[TcpPrintService] No IP address provided.");
            return false;
        }

        Debug.WriteLine($"[TcpPrintService] Printing to: {ip}:{port}");
        Debug.WriteLine($"[TcpPrintService] Copies: {copies}");
        Debug.WriteLine($"[TcpPrintService] ZPL length: {zpl.Length} chars");

        for (int copy = 1; copy <= copies; copy++)
        {
            if (copy > 1)
                Debug.WriteLine($"[TcpPrintService] Printing copy {copy}/{copies}");

            try
            {
                var client = new TcpClient();
                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(10000); // 10 second timeout

                await client.ConnectAsync(ip, port);

                using var stream = client.GetStream();
                var bytes = System.Text.Encoding.ASCII.GetBytes(zpl);

                await stream.WriteAsync(bytes, 0, bytes.Length);
                await stream.FlushAsync();

                Debug.WriteLine($"[TcpPrintService] Copy {copy} sent: {bytes.Length} bytes to {ip}:{port}");

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TcpPrintService] Copy {copy} failed: {ex.Message}");
                if (copy == 1)
                {
                    throw;
                }
                return false;
            }
        }

        return true;
    }
}
