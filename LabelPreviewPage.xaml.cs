using Microsoft.Data.SqlClient;
using ZXing;
using ZXing.QrCode;
using ZXing.Net.Maui;
using Android.Widget;
using Android.Content;
using Android.Graphics;

namespace PrintLabels;

public partial class LabelPreviewPage : ContentPage
{
    private int _quantity = 1;
    private readonly MainPage _mainPage;
    private readonly string _itemNumber;
    private readonly string _itemCodeDesc;
    private readonly string _searchTerm;
    private readonly string _facility;
    private readonly string _warehouse;
    private readonly string _aisle;
    private readonly string _column;
    private readonly string _level;
    private readonly string _arrow;
    private readonly string _spot;
    private readonly string _comment;
    private readonly string _ver1;
    private readonly string _ver2;
    private readonly string _ver3;
    private readonly string _ver4;
    private readonly string _ver5;
    private readonly string _ver6;
    private readonly string _ver7;

    public LabelPreviewPage(
        MainPage mainPage,
        string itemNumber,
        string itemCodeDesc,
        string searchTerm,
        string facility,
        string warehouse,
        string aisle,
        string column,
        string level,
        string spot,
        string comment,
        string ver1,
        string ver2,
        string ver3,
        string ver4,
        string ver5,
        string ver6,
        string ver7)
    {
        _mainPage = mainPage;
        _itemNumber = itemNumber ?? "";
        _itemCodeDesc = itemCodeDesc ?? "";
        _searchTerm = searchTerm ?? "";
        _facility = facility ?? "";
        _warehouse = warehouse ?? "";
        _aisle = aisle ?? "";
        _column = column ?? "";
        _level = level ?? "";
        _spot = spot ?? "";
        _comment = comment ?? "";
        _ver1 = ver1 ?? "";
        _ver2 = ver2 ?? "";
        _ver3 = ver3 ?? "";
        _ver4 = ver4 ?? "";
        _ver5 = ver5 ?? "";
        _ver6 = ver6 ?? "";
        _ver7 = ver7 ?? "";

        InitializeComponent();
        SetLabelPreviewValues();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetLabelPreviewValues();
    }

    private async void SetLabelPreviewValues()
    {
        lblVer1.Text = _ver1;
        lblVer2.Text = _ver2;
        lblVer3.Text = _ver3;
        lblVer4.Text = _ver4;
        lblVer5.Text = _ver5;
        lblVer6.Text = _ver6;
        lblVer7.Text = _ver7;

        lblWarehouse.Text = _warehouse;
        lblAisle.Text = _aisle;
        lblColumn.Text = _column;
        lblLevel.Text = _level;
        lblSpot.Text = _spot;

        lblItemCodeDesc.Text = _itemCodeDesc;
        lblItemNumber.Text = _itemNumber;
        lblComment.Text = _comment;

        GenerateQRCode();
    }

    private async void GenerateQRCode()
    {
        qrCodeImage.Source = null;
    }

    private async void OnQuantityMinusClicked(object sender, EventArgs e)
    {
        if (_quantity > 1)
        {
            _quantity--;
            QuantityLabel.Text = _quantity.ToString();
        }
    }

    private async void OnQuantityPlusClicked(object sender, EventArgs e)
    {
        if (_quantity < 99)
        {
            _quantity++;
            QuantityLabel.Text = _quantity.ToString();
        }
    }

    private async void OnPrintNowClicked(object sender, EventArgs e)
    {
        try
        {
            string zpl = GenerateZPL();
            System.Diagnostics.Debug.WriteLine("[PRINT] Arrow: " + _level + " -> " + ((_level == "1") ? "v (down)" : "u (up)"));
            System.Diagnostics.Debug.WriteLine("[PRINT] Full ZPL:\n" + zpl);

            var success = await PrintLabels.Services.BtPrintService.PrintAsync(zpl, _quantity);

            if (!success)
            {
                Toast.MakeText(Android.App.Application.Context, "Print FAILED - check printer settings", ToastLength.Long).Show();
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(Android.App.Application.Context, "ERROR: " + ex.Message, ToastLength.Long).Show();
        }
    }

    private async void OnIPrintNowClicked(object sender, EventArgs e)
    {
        try
        {
            string zpl = GenerateZPL();
            string ip = "192.168.210.164";
            
            var success = await PrintLabels.Services.TcpPrintService.PrintAsync(ip, 9100, zpl, _quantity);

            if (!success)
            {
                Toast.MakeText(Android.App.Application.Context, "IPPrint FAILED - check printer connection", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(Android.App.Application.Context, "IPPrint sent successfully", ToastLength.Short).Show();
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(Android.App.Application.Context, "IPPrint ERROR: " + ex.Message, ToastLength.Long).Show();
        }
    }

    private string GenerateZPL()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("^XA");
        sb.AppendLine("^MMT");
        sb.AppendLine("^PW812");
        sb.AppendLine("^LL1218");
        sb.AppendLine("^LS0");

        // Day labels - using ^FH\ (single backslash)
        string fh = "^FH\\\\";
        sb.AppendLine("^FT78,126^A0B,34,33" + fh + "^FDSa^FS");
        sb.AppendLine("^FT80,305^A0B,34,33" + fh + "^FDFr^FS");
        sb.AppendLine("^FT80,474^A0B,34,33" + fh + "^FDTh^FS");
        sb.AppendLine("^FT80,652^A0B,34,33" + fh + "^FDWe^FS");
        sb.AppendLine("^FT79,816^A0B,34,33" + fh + "^FDTu^FS");
        sb.AppendLine("^FT75,992^A0B,34,33" + fh + "^FDMo^FS");
        sb.AppendLine("^FT74,1168^A0B,34,33" + fh + "^FDSu^FS");

        // Day values
        sb.AppendLine("^FT159,155^A0B,90,88" + fh + "^FD" + _ver7 + "^FS");
        sb.AppendLine("^FT156,332^A0B,90,88" + fh + "^FD" + _ver6 + "^FS");
        sb.AppendLine("^FT156,503^A0B,90,88" + fh + "^FD" + _ver5 + "^FS");
        sb.AppendLine("^FT156,680^A0B,90,88" + fh + "^FD" + _ver4 + "^FS");
        sb.AppendLine("^FT155,847^A0B,90,88" + fh + "^FD" + _ver3 + "^FS");
        sb.AppendLine("^FT154,1019^A0B,90,88" + fh + "^FD" + _ver2 + "^FS");
        sb.AppendLine("^FT154,1195^A0B,90,88" + fh + "^FD" + _ver1 + "^FS");

        // Location labels
        sb.AppendLine("^FT217,896^A0B,34,33" + fh + "^FDAISLE^FS");
        sb.AppendLine("^FT217,1157^A0B,34,33" + fh + "^FDWAREHOUSE^FS");
        sb.AppendLine("^FT217,272^A0B,34,33" + fh + "^FDSPOT^FS");
        sb.AppendLine("^FT213,451^A0B,34,33" + fh + "^FDLEVEL^FS");
        sb.AppendLine("^FT218,689^A0B,34,33" + fh + "^FDCOLUMN^FS");

        // Location values
        sb.AppendLine("^FT331,278^A0B,135,134" + fh + "^FD" + _spot + "^FS");
        sb.AppendLine("^FT332,455^A0B,135,134" + fh + "^FD" + _level + "^FS");
        sb.AppendLine("^FT334,691^A0B,135,134" + fh + "^FD" + _column + "^FS");
        sb.AppendLine("^FT332,920^A0B,135,134" + fh + "^FD" + _aisle + "^FS");
        sb.AppendLine("^FT331,1135^A0B,135,134" + fh + "^FD" + _warehouse + "^FS");

        // Product info
        sb.AppendLine("^FT464,820^A0B,56,55" + fh + "^FD" + _itemCodeDesc + "^FS");
        sb.AppendLine("^FT464,1054^A0B,56,55" + fh + "^FD" + _itemNumber + "^FS");

        // Arrow: down arrow for level 1 (V-shape using font B 'o' and 'x' characters)
        // Arrow: up arrow for levels 2+ (inverted V-shape)
        if (_level == "1")
        {
            // DOWN arrow
            sb.AppendLine("^FT360,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT360,100^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT330,85^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT330,85^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT330,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT330,100^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT330,115^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT330,115^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT300,70^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT300,70^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT300,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT300,100^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT300,130^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT300,130^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT270,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT270,100^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT240,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT240,100^A0B,34,33" + fh + "^FDx^FS");
        }
        else
        {
            // UP arrow
            sb.AppendLine("^FT360,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT360,100^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT330,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT330,100^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT300,70^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT300,70^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT300,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT300,100^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT300,130^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT300,130^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT270,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT270,100^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT240,100^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT240,100^A0B,34,33" + fh + "^FDx^FS");

            sb.AppendLine("^FT270,85^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT270,85^A0B,34,33" + fh + "^FDx^FS");
            sb.AppendLine("^FT270,115^A0B,34,33" + fh + "^FDo^FS");
            sb.AppendLine("^FT270,115^A0B,34,33" + fh + "^FDx^FS");
        }

        // QR Code
        sb.AppendLine("^FO384,1056^BQN,2,4^FDMA," + _itemNumber + "^FS");

        // Divider lines
        sb.AppendLine("^FO13,1041^GB156,0,8^FS");
        sb.AppendLine("^FO14,872^GB156,0,8^FS");
        sb.AppendLine("^FO13,699^GB158,0,8^FS");
        sb.AppendLine("^FO13,527^GB159,0,8^FS");
        sb.AppendLine("^FO11,353^GB159,0,8^FS");
        sb.AppendLine("^FO5,183^GB166,0,8^FS");

        sb.AppendLine("^PQ1,0,1,Y");
        sb.AppendLine("^XZ");

        return sb.ToString();
    }

    private async void OnPrintAnotherClicked(object sender, EventArgs e)
    {
        _quantity = 1;
        QuantityLabel.Text = "1";
        await Navigation.PopAsync();
        await _mainPage?.FocusEntryAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private string GenerateQRBmp(string data)
    {
        try
        {
            var options = new ZXing.Common.EncodingOptions
            {
                Width = 100,
                Height = 100,
                Margin = 0
            };

            var writer = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = options
            };

            var result = writer.Write(data);

            if (result == null || result.Pixels == null || result.Pixels.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[QR] ERROR: result is null/empty");
                return "";
            }

            System.Diagnostics.Debug.WriteLine("[QR] Pixels.Length=" + result.Pixels.Length);
            System.Diagnostics.Debug.WriteLine("[QR] Width=" + result.Width + " Height=" + result.Height);

            var debugHex = new System.Text.StringBuilder();
            for (int i = 0; i < Math.Min(25, result.Pixels.Length); i++)
            {
                debugHex.AppendFormat("{0:X8} ", result.Pixels[i]);
            }
            System.Diagnostics.Debug.WriteLine("[QR] first 25 pixels: " + debugHex.ToString());

            int blackCount = 0;
            int nonBlackCount = 0;
            for (int i = 0; i < result.Pixels.Length; i++)
            {
                if (result.Pixels[i] == 0) blackCount++;
                else nonBlackCount++;
            }
            System.Diagnostics.Debug.WriteLine("[QR] all-black=" + blackCount + " non-black=" + nonBlackCount);

            int size = result.Width;
            int bytesPerRow = (size + 7) / 8;
            byte[] rawBytes = new byte[size * bytesPerRow];

            int darkCount = 0;
            int lightCount = 0;
            var debugModules = new System.Text.StringBuilder();

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int idx = y * size + x;
                    int pixel = result.Pixels[idx];

                    bool isDark1 = pixel != 0;
                    bool isDark2 = (pixel & 0x00FFFFFF) != 0x00FFFFFF;
                    int r = (pixel >> 16) & 0xFF;
                    int g = (pixel >> 8) & 0xFF;
                    int b = pixel & 0xFF;
                    bool isDark3 = (r + g + b) < 384;

                    bool isDark = isDark1;

                    if (y == 0 && x < 25)
                    {
                        debugModules.AppendFormat(isDark ? "1" : "0");
                        if (x == 24)
                        {
                            System.Diagnostics.Debug.WriteLine("[QR] row0 dark1=" + darkCount + " dark2=" + darkCount + " dark3=" + darkCount + " r=" + r + " g=" + g + " b=" + b);
                        }
                    }

                    if (isDark) darkCount++;
                    else lightCount++;

                    int bytePos = y * bytesPerRow + (x / 8);
                    int bitPos = 7 - (x % 8);

                    if (isDark)
                    {
                        rawBytes[bytePos] |= (byte)(1 << bitPos);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("[QR] dark=" + darkCount + " light=" + lightCount);
            System.Diagnostics.Debug.WriteLine("[QR] first row: " + debugModules.ToString());
            System.Diagnostics.Debug.WriteLine("[QR] size=" + size + " BPR=" + bytesPerRow);

            var hexSb = new System.Text.StringBuilder();
            foreach (byte b in rawBytes)
            {
                hexSb.AppendFormat("{0:x2}", b);
            }
            string hexData = hexSb.ToString();

            System.Diagnostics.Debug.WriteLine("[QR] hexLen=" + hexData.Length);
            System.Diagnostics.Debug.WriteLine("[QR] first 64: " + hexData.Substring(0, Math.Min(64, hexData.Length)));
            return hexData;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("[QR] EXCEPTION: " + ex.Message);
            System.Diagnostics.Debug.WriteLine("[QR] Type: " + ex.GetType().Name);
            return "";
        }
    }
}
