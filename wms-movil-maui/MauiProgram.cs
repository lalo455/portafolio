using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace WmsMovil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            // === MAGIA PARA QUITAR LA LÍNEA FEA DE TODOS LOS INPUTS EN ANDROID ===
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });
            // ==================================================================================================================================



#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Registramos el servicio RFID para poder inyectarlo en las pantallas
            builder.Services.AddSingleton<WmsMovil.Services.IRfidService, WmsMovil.Services.RfidService>();
            builder.Services.AddTransient<WmsMovil.ViewModels.ScanViewModel>();
            builder.Services.AddTransient<WmsMovil.ScanPage>();
            builder.Services.AddSingleton<WmsMovil.Services.ApiService>();
            builder.Services.AddTransient<MainPage>();
            // Servicios
            builder.Services.AddSingleton<WmsMovil.Services.ApiService>();
            builder.Services.AddSingleton<WmsMovil.Services.IRfidService, WmsMovil.Services.RfidService>();
            builder.Services.AddSingleton<WmsMovil.Services.ScannerOrchestrator>();

            // Pantallas y ViewModels
            builder.Services.AddTransient<WmsMovil.ViewModels.LoginViewModel>();
            builder.Services.AddTransient<WmsMovil.MainPage>();
            builder.Services.AddTransient<WmsMovil.ViewModels.MenuViewModel>();
         
            builder.Services.AddTransient<WmsMovil.ViewModels.ScanViewModel>();
            builder.Services.AddTransient<WmsMovil.ScanPage>();
            builder.Services.AddTransient<WmsMovil.MenuPage>();
            builder.Services.AddTransient<WmsMovil.TraspasoPage>();
            builder.Services.AddTransient<WmsMovil.ViewModels.TraspasoViewModel>();
            builder.Services.AddSingleton<App>();
            return builder.Build();
        }
    }
}
