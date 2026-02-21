using System.Collections.ObjectModel;
using Logicore.Models;
using Logicore.Services;
using Logicore.Services;

namespace Logicore.ViewModels
{
    public class VehicleAnalytic
    {
        public string VehicleName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public double TotalFuelCost { get; set; }
        public double TotalMaintenanceCost { get; set; }
        public double TotalRevenue { get; set; }
        public double AcquisitionCost { get; set; }
        public double TotalDistanceKm { get; set; }
        public double TotalLiters { get; set; }

        public double TotalOperationalCost => TotalFuelCost + TotalMaintenanceCost;
        public double NetProfit => TotalRevenue - TotalOperationalCost;
        public double ROI => AcquisitionCost > 0
            ? Math.Round((TotalRevenue - TotalOperationalCost) / AcquisitionCost * 100, 2)
            : 0;
        public double FuelEfficiency => TotalLiters > 0
            ? Math.Round(TotalDistanceKm / TotalLiters, 2)
            : 0;
        public double CostPerKm => TotalDistanceKm > 0
            ? Math.Round(TotalOperationalCost / TotalDistanceKm, 2)
            : 0;

        public string ROIColor => ROI >= 0 ? "#2ecc71" : "#e74c3c";
        public string ProfitColor => NetProfit >= 0 ? "#2ecc71" : "#e74c3c";
    }

    public class AnalyticsViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private double _totalFleetRevenue;
        private double _totalFleetExpenses;
        private double _averageROI;
        private double _fleetUtilization;

        public ObservableCollection<VehicleAnalytic> VehicleAnalytics { get; } = new();

        public double TotalFleetRevenue { get => _totalFleetRevenue; set => SetProperty(ref _totalFleetRevenue, value); }
        public double TotalFleetExpenses { get => _totalFleetExpenses; set => SetProperty(ref _totalFleetExpenses, value); }
        public double AverageROI { get => _averageROI; set => SetProperty(ref _averageROI, value); }
        public double FleetUtilization { get => _fleetUtilization; set => SetProperty(ref _fleetUtilization, value); }
        public double NetFleetProfit => TotalFleetRevenue - TotalFleetExpenses;
        public string NetProfitColor => NetFleetProfit >= 0 ? "#2ecc71" : "#e74c3c";

        public AsyncRelayCommand LoadCommand { get; }

        public AnalyticsViewModel(ApiService api)
        {
            _api = api;
            Title = "Analytics & Reports";
            LoadCommand = new AsyncRelayCommand(LoadAnalyticsAsync);
        }

        public async Task LoadAnalyticsAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                // Load all data in parallel
                var vehiclesTask = _api.GetVehiclesAsync();
                var tripsTask = _api.GetTripsAsync("Completed");
                var fuelTask = _api.GetFuelLogsAsync();
                var maintenanceTask = _api.GetMaintenanceLogsAsync();
                var revenueTask = _api.GetRevenueAsync();
                var statsTask = _api.GetDashboardStatsAsync();

                await Task.WhenAll(vehiclesTask, tripsTask, fuelTask, maintenanceTask, revenueTask, statsTask);

                var vehicles = vehiclesTask.Result.Data?.Results ?? new();
                var trips = tripsTask.Result.Data?.Results ?? new();
                var fuelLogs = fuelTask.Result.Data?.Results ?? new();
                var maintenanceLogs = maintenanceTask.Result.Data?.Results ?? new();
                var revenues = revenueTask.Result.Data?.Results ?? new();

                // Build per-vehicle analytics
                VehicleAnalytics.Clear();
                foreach (var v in vehicles)
                {
                    var vTrips = trips.Where(t => t.VehicleId == v.VehicleId).ToList();
                    var vFuel = fuelLogs.Where(f => f.VehicleId == v.VehicleId).ToList();
                    var vMaint = maintenanceLogs.Where(m => m.VehicleId == v.VehicleId).ToList();
                    var tripIds = vTrips.Select(t => t.TripId).ToHashSet();
                    var vRevenue = revenues.Where(r => tripIds.Contains(r.TripId)).ToList();

                    var distanceKm = vTrips
                        .Where(t => t.EndOdometer.HasValue)
                        .Sum(t => t.EndOdometer!.Value - t.StartOdometer);

                    VehicleAnalytics.Add(new VehicleAnalytic
                    {
                        VehicleName = v.VehicleName,
                        LicensePlate = v.LicensePlate,
                        TotalFuelCost = vFuel.Sum(f => f.Cost),
                        TotalMaintenanceCost = vMaint.Sum(m => m.Cost),
                        TotalRevenue = vRevenue.Sum(r => r.RevenueAmount),
                        AcquisitionCost = v.AcquisitionCost,
                        TotalDistanceKm = distanceKm,
                        TotalLiters = vFuel.Sum(f => f.Liters)
                    });
                }

                // Fleet totals
                TotalFleetRevenue = VehicleAnalytics.Sum(a => a.TotalRevenue);
                TotalFleetExpenses = VehicleAnalytics.Sum(a => a.TotalOperationalCost);
                AverageROI = VehicleAnalytics.Any() ? VehicleAnalytics.Average(a => a.ROI) : 0;
                FleetUtilization = statsTask.Result.Data?.UtilizationRate ?? 0;

                OnPropertyChanged(nameof(NetFleetProfit));
                OnPropertyChanged(nameof(NetProfitColor));
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            finally { IsBusy = false; }
        }
    }
}
