using Microsoft.Extensions.Logging;
using HolaMundo.Services;
using HolaMundo.ViewModels;
using HolaMundo.Views;

namespace HolaMundo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Servicios (Singleton)
            builder.Services.AddSingleton<SesionActual>();
            builder.Services.AddSingleton<ParadasDatabase>();
            builder.Services.AddSingleton<RepartidorDatabase>();
            builder.Services.AddSingleton<TurnoDatabase>();
            builder.Services.AddSingleton<UbicacionService>();

            // ViewModels y Páginas (Transient)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddTransient<JornadaViewModel>();
            builder.Services.AddTransient<JornadaPage>();

            builder.Services.AddTransient<HistoricoViewModel>();
            builder.Services.AddTransient<HistoricoPage>();

            builder.Services.AddTransient<ParadaDetalleViewModel>();
            builder.Services.AddTransient<ParadaDetallePage>();

            return builder.Build();
        }
    }
}
