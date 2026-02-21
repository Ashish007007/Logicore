using System.Collections.ObjectModel;
using Logicore.Models;
using Logicore.Services;

namespace Logicore.ViewModels
{
    public class TripViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private bool _isFormVisible;
        private string _filterStatus = "All";

        // Form fields
        private int _formVehicleId;
        private int _formDriverId;
        private double _formCargoWeight;
        private string _formOrigin = string.Empty;
        private string _formDestination = string.Empty;
        private DateTime _formStartDate = DateTime.Today;
        private double _formStartOdometer;
        private double _formEndOdometer;
        private Trip? _completingTrip;

        public ObservableCollection<Trip> Trips { get; } = new();
        public ObservableCollection<Vehicle> AvailableVehicles { get; } = new();
        public ObservableCollection<Driver> AvailableDrivers { get; } = new();

        public bool IsFormVisible { get => _isFormVisible; set => SetProperty(ref _isFormVisible, value); }
        public string FilterStatus { get => _filterStatus; set => SetProperty(ref _filterStatus, value); }

        public int FormVehicleId { get => _formVehicleId; set => SetProperty(ref _formVehicleId, value); }
        public int FormDriverId { get => _formDriverId; set => SetProperty(ref _formDriverId, value); }
        public double FormCargoWeight { get => _formCargoWeight; set => SetProperty(ref _formCargoWeight, value); }
        public string FormOrigin { get => _formOrigin; set => SetProperty(ref _formOrigin, value); }
        public string FormDestination { get => _formDestination; set => SetProperty(ref _formDestination, value); }
        public DateTime FormStartDate { get => _formStartDate; set => SetProperty(ref _formStartDate, value); }
        public double FormStartOdometer { get => _formStartOdometer; set => SetProperty(ref _formStartOdometer, value); }
        public double FormEndOdometer { get => _formEndOdometer; set => SetProperty(ref _formEndOdometer, value); }
        public Trip? CompletingTrip { get => _completingTrip; set { SetProperty(ref _completingTrip, value); OnPropertyChanged(nameof(IsCompletingTrip)); } }
        public bool IsCompletingTrip => CompletingTrip != null;

        public List<string> StatusFilters { get; } = new() { "All", "Draft", "Dispatched", "Completed", "Cancelled" };

        public AsyncRelayCommand LoadCommand { get; }
        public RelayCommand ShowAddFormCommand { get; }
        public RelayCommand HideFormCommand { get; }
        public AsyncRelayCommand DispatchTripCommand { get; }
        public AsyncRelayCommand CompleteTripCommand { get; }
        public AsyncRelayCommand CancelTripCommand { get; }

        public TripViewModel(ApiService api)
        {
            _api = api;
            Title = "Trip Dispatcher";
            LoadCommand = new AsyncRelayCommand(LoadDataAsync);
            ShowAddFormCommand = new RelayCommand(async _ => await PrepareFormAsync());
            HideFormCommand = new RelayCommand(_ => { IsFormVisible = false; CompletingTrip = null; });
            DispatchTripCommand = new AsyncRelayCommand(DispatchTripAsync);
            CompleteTripCommand = new AsyncRelayCommand(async p => await FinishTripAsync(p as Trip));
            CancelTripCommand = new AsyncRelayCommand(async p => await CancelTripAsync(p as Trip));
        }

        public async Task LoadDataAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var status = FilterStatus == "All" ? null : FilterStatus;
                var result = await _api.GetTripsAsync(status);
                Trips.Clear();
                if (result.Success && result.Data != null)
                    foreach (var t in result.Data.Results)
                        Trips.Add(t);
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task PrepareFormAsync()
        {
            var vTask = _api.GetVehiclesAsync("Available");
            var dTask = _api.GetDriversAsync("Off Duty");
            await Task.WhenAll(vTask, dTask);

            AvailableVehicles.Clear();
            if (vTask.Result.Success && vTask.Result.Data != null)
                foreach (var v in vTask.Result.Data.Results)
                    AvailableVehicles.Add(v);

            AvailableDrivers.Clear();
            if (dTask.Result.Success && dTask.Result.Data != null)
                foreach (var d in dTask.Result.Data.Results)
                    AvailableDrivers.Add(d);

            IsFormVisible = true;
        }

        private async Task DispatchTripAsync()
        {
            // Validate cargo weight against vehicle capacity
            var vehicle = AvailableVehicles.FirstOrDefault(v => v.VehicleId == FormVehicleId);
            if (vehicle != null && FormCargoWeight > vehicle.MaxCapacityKg)
            {
                ErrorMessage = $"⚠️ Cargo weight ({FormCargoWeight}kg) exceeds vehicle capacity ({vehicle.MaxCapacityKg}kg)!";
                return;
            }

            if (string.IsNullOrWhiteSpace(FormOrigin) || string.IsNullOrWhiteSpace(FormDestination))
            {
                ErrorMessage = "Origin and destination are required.";
                return;
            }

            IsBusy = true;
            ClearError();
            try
            {
                var req = new TripCreateRequest
                {
                    VehicleId = FormVehicleId,
                    DriverId = FormDriverId,
                    CargoWeight = FormCargoWeight,
                    Origin = FormOrigin,
                    Destination = FormDestination,
                    StartDate = FormStartDate,
                    StartOdometer = FormStartOdometer
                };
                var result = await _api.CreateTripAsync(req);
                if (result.Success)
                {
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task FinishTripAsync(Trip? trip)
        {
            if (trip == null) return;
            CompletingTrip = trip;
            FormEndOdometer = trip.StartOdometer;
        }

        public async Task SubmitCompletionAsync()
        {
            if (CompletingTrip == null) return;
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _api.CompleteTripAsync(CompletingTrip.TripId, FormEndOdometer);
                if (result.Success)
                {
                    CompletingTrip = null;
                    await LoadDataAsync();
                }
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task CancelTripAsync(Trip? trip)
        {
            if (trip == null) return;
            var confirm = await Shell.Current.DisplayAlert("Cancel Trip",
                $"Cancel trip {trip.Origin} → {trip.Destination}?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            var result = await _api.UpdateTripStatusAsync(trip.TripId, "Cancelled");
            if (result.Success)
                await LoadDataAsync();
            else
                ErrorMessage = result.Message;
            IsBusy = false;
        }
    }
}
