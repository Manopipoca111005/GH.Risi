using CommunityToolkit.Maui;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;
using MauiApp1.Services;
// #if ANDROID // Removido o using para Platforms.Android, pois não é estritamente necessário se o compilador conseguir resolver o tipo
// using MauiApp1.Platforms.Android;
// #endif

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-PT");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-PT");
#if ANDROID
            builder.Services.AddSingleton<ICalendarService, AndroidCalendarService>();
#endif
            return builder.Build();
        }
    }
}