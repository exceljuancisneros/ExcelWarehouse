using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace PrintLabels;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            })
            .UseBarcodeReader();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
