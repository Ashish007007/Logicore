using CommunityToolkit.Maui;
using Logicore.ViewModels;
using Logicore.Interfaces;
using Logicore.Models;
using Logicore.Services;
using Logicore.View;
using Microsoft.Extensions.Logging;

namespace Logicore
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

        

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // ── Services ───────────────────────────────────────────────────────
            builder.Services.AddSingleton<ApiService>();

            // ── ViewModels ────────────────────────────────────────────────────
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<VehicleViewModel>();
            builder.Services.AddSingleton<TripViewModel>();
            builder.Services.AddSingleton<DriverViewModel>();
            builder.Services.AddSingleton<MaintenanceViewModel>();
            builder.Services.AddSingleton<ExpensesViewModel>();
            builder.Services.AddSingleton<AnalyticsViewModel>();

            // ── Views ─────────────────────────────────────────────────────────
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddSingleton<VehiclePage>();
            builder.Services.AddSingleton<TripPage>();
            builder.Services.AddSingleton<DriverPage>();
            builder.Services.AddSingleton<MaintenancePage>();
            builder.Services.AddSingleton<ExpensesPage>();
            builder.Services.AddSingleton<AnalyticsPage>();

            return builder.Build();

            return builder.Build();
        }

    }
}
