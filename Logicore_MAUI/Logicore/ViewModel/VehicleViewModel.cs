using System.Collections.ObjectModel;
using Logicore.Models;
using Logicore.Services;

namespace Logicore.ViewModels
{
    public class VehicleViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private Vehicle? _selectedVehicle;
        private string _filterStatus = "All";

        // ─── Form fields ────────────────────────────────────────────────
        private string _formVehicleName = string.Empty;
        private string _formVehicleType = "Truck";
        private string _formLicensePlate = string.Empty;
        private double _formMaxCapacity;
        private double _formOdometer;
        private double _formAcquisitionCost;
        private bool _isFormVisible;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set { SetProperty(ref _selectedVehicle, value); LoadFormFromSelected(); }
        }

        public string FilterStatus { get => _filterStatus; set { SetProperty(ref _filterStatus, value); } }
        public string FormVehicleName { get => _formVehicleName; set => SetProperty(ref _formVehicleName, value); }
        public string FormVehicleType { get => _formVehicleType; set => SetProperty(ref _formVehicleType, value); }
        public string FormLicensePlate { get => _formLicensePlate; set => SetProperty(ref _formLicensePlate, value); }
        public double FormMaxCapacity { get => _formMaxCapacity; set => SetProperty(ref _formMaxCapacity, value); }
        public double FormOdometer { get => _formOdometer; set => SetProperty(ref _formOdometer, value); }
        public double FormAcquisitionCost { get => _formAcquisitionCost; set => SetProperty(ref _formAcquisitionCost, value); }
        public bool IsFormVisible { get => _isFormVisible; set { SetProperty(ref _isFormVisible, value); OnPropertyChanged(nameof(FormTitle)); } }
        public string FormTitle => SelectedVehicle == null ? "Add New Vehicle" : "Edit Vehicle";
        public bool IsEditMode => SelectedVehicle != null;

        public List<string> VehicleTypes { get; } = new() { "Truck", "Van", "Bike", "Bus" };
        public List<string> StatusFilters { get; } = new() { "All", "Available", "On Trip", "In Shop", "Retired" };

        public AsyncRelayCommand LoadCommand { get; }
        public RelayCommand ShowAddFormCommand { get; }
        public RelayCommand HideFormCommand { get; }
        public AsyncRelayCommand SaveVehicleCommand { get; }
        public AsyncRelayCommand DeleteVehicleCommand { get; }

        public VehicleViewModel(ApiService api)
        {
            _api = api;
            Title = "Vehicle Registry";

            LoadCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            ShowAddFormCommand = new RelayCommand(_ => { SelectedVehicle = null; ClearForm(); IsFormVisible = true; });
            HideFormCommand = new RelayCommand(_ => { IsFormVisible = false; ClearForm(); });
            SaveVehicleCommand = new AsyncRelayCommand(SaveVehicleAsync);
            DeleteVehicleCommand = new AsyncRelayCommand(async p => await DeleteVehicleAsync(p as Vehicle));
        }

        public async Task LoadVehiclesAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var status = FilterStatus == "All" ? null : FilterStatus;
                var result = await _api.GetVehiclesAsync(status);
                Vehicles.Clear();
                if (result.Success && result.Data != null)
                    foreach (var v in result.Data.Results)
                        Vehicles.Add(v);
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task SaveVehicleAsync()
        {
            if (string.IsNullOrWhiteSpace(FormVehicleName) || string.IsNullOrWhiteSpace(FormLicensePlate))
            {
                ErrorMessage = "Vehicle name and license plate are required.";
                return;
            }
            IsBusy = true;
            ClearError();
            try
            {
                var v = new Vehicle
                {
                    VehicleName = FormVehicleName,
                    VehicleType = FormVehicleType,
                    LicensePlate = FormLicensePlate,
                    MaxCapacityKg = FormMaxCapacity,
                    Odometer = FormOdometer,
                    AcquisitionCost = FormAcquisitionCost,
                    Status = SelectedVehicle?.Status ?? "Available"
                };

                ApiResponse<Vehicle> result;
                if (SelectedVehicle != null)
                    result = await _api.UpdateVehicleAsync(SelectedVehicle.VehicleId, v);
                else
                    result = await _api.CreateVehicleAsync(v);

                if (result.Success)
                {
                    IsFormVisible = false;
                    await LoadVehiclesAsync();
                }
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task DeleteVehicleAsync(Vehicle? vehicle)
        {
            if (vehicle == null) return;
            var confirm = await Shell.Current.DisplayAlert("Delete Vehicle",
                $"Are you sure you want to delete {vehicle.VehicleName}?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            var ok = await _api.DeleteVehicleAsync(vehicle.VehicleId);
            if (ok) Vehicles.Remove(vehicle);
            else ErrorMessage = "Failed to delete vehicle.";
            IsBusy = false;
        }

        private void LoadFormFromSelected()
        {
            if (SelectedVehicle == null) return;
            FormVehicleName = SelectedVehicle.VehicleName;
            FormVehicleType = SelectedVehicle.VehicleType;
            FormLicensePlate = SelectedVehicle.LicensePlate;
            FormMaxCapacity = SelectedVehicle.MaxCapacityKg;
            FormOdometer = SelectedVehicle.Odometer;
            FormAcquisitionCost = SelectedVehicle.AcquisitionCost;
            IsFormVisible = true;
        }

        private void ClearForm()
        {
            FormVehicleName = string.Empty;
            FormVehicleType = "Truck";
            FormLicensePlate = string.Empty;
            FormMaxCapacity = 0;
            FormOdometer = 0;
            FormAcquisitionCost = 0;
        }
    }
}
