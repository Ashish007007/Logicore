using System.Collections.ObjectModel;
using Logicore.Models;
using Logicore.Services;

namespace Logicore.ViewModels
{
    public class DriverViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private bool _isFormVisible;
        private Driver? _selectedDriver;

        // Form fields
        private string _formDriverName = string.Empty;
        private string _formLicenseNumber = string.Empty;
        private string _formLicenseCategory = "Van";
        private DateTime _formLicenseExpiry = DateTime.Today.AddYears(1);
        private double _formSafetyScore = 100;

        public ObservableCollection<Driver> Drivers { get; } = new();

        public bool IsFormVisible { get => _isFormVisible; set => SetProperty(ref _isFormVisible, value); }
        public Driver? SelectedDriver { get => _selectedDriver; set { SetProperty(ref _selectedDriver, value); LoadFormFromSelected(); } }

        public string FormDriverName { get => _formDriverName; set => SetProperty(ref _formDriverName, value); }
        public string FormLicenseNumber { get => _formLicenseNumber; set => SetProperty(ref _formLicenseNumber, value); }
        public string FormLicenseCategory { get => _formLicenseCategory; set => SetProperty(ref _formLicenseCategory, value); }
        public DateTime FormLicenseExpiry { get => _formLicenseExpiry; set => SetProperty(ref _formLicenseExpiry, value); }
        public double FormSafetyScore { get => _formSafetyScore; set => SetProperty(ref _formSafetyScore, value); }
        public string FormTitle => SelectedDriver == null ? "Add New Driver" : "Edit Driver";

        public List<string> LicenseCategories { get; } = new() { "Truck", "Van", "Bike", "Bus" };

        public AsyncRelayCommand LoadCommand { get; }
        public RelayCommand ShowAddFormCommand { get; }
        public RelayCommand HideFormCommand { get; }
        public AsyncRelayCommand SaveDriverCommand { get; }
        public AsyncRelayCommand ToggleStatusCommand { get; }

        public DriverViewModel(ApiService api)
        {
            _api = api;
            Title = "Driver Profiles";
            LoadCommand = new AsyncRelayCommand(LoadDriversAsync);
            ShowAddFormCommand = new RelayCommand(_ => { SelectedDriver = null; ClearForm(); IsFormVisible = true; });
            HideFormCommand = new RelayCommand(_ => { IsFormVisible = false; ClearForm(); });
            SaveDriverCommand = new AsyncRelayCommand(SaveDriverAsync);
            ToggleStatusCommand = new AsyncRelayCommand(async p => await ToggleDriverStatusAsync(p as Driver));
        }

        public async Task LoadDriversAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _api.GetDriversAsync();
                Drivers.Clear();
                if (result.Success && result.Data != null)
                    foreach (var d in result.Data.Results)
                        Drivers.Add(d);
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task SaveDriverAsync()
        {
            if (string.IsNullOrWhiteSpace(FormDriverName) || string.IsNullOrWhiteSpace(FormLicenseNumber))
            {
                ErrorMessage = "Driver name and license number are required.";
                return;
            }

            // Check license expiry
            if (FormLicenseExpiry < DateTime.Today)
            {
                ErrorMessage = "⚠️ License has already expired! Please update the expiry date.";
                return;
            }

            IsBusy = true;
            ClearError();
            try
            {
                var d = new Driver
                {
                    DriverName = FormDriverName,
                    LicenseNumber = FormLicenseNumber,
                    LicenseCategory = FormLicenseCategory,
                    LicenseExpiry = FormLicenseExpiry,
                    SafetyScore = FormSafetyScore,
                    Status = SelectedDriver?.Status ?? "Off Duty"
                };

                ApiResponse<Driver> result;
                if (SelectedDriver != null)
                    result = await _api.UpdateDriverAsync(SelectedDriver.DriverId, d);
                else
                    result = await _api.CreateDriverAsync(d);

                if (result.Success)
                {
                    IsFormVisible = false;
                    await LoadDriversAsync();
                }
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task ToggleDriverStatusAsync(Driver? driver)
        {
            if (driver == null) return;
            var newStatus = driver.Status == "On Duty" ? "Off Duty" : "On Duty";
            var result = await _api.UpdateDriverAsync(driver.DriverId, new Driver { Status = newStatus });
            if (result.Success) await LoadDriversAsync();
            else ErrorMessage = result.Message;
        }

        private void LoadFormFromSelected()
        {
            if (SelectedDriver == null) return;
            FormDriverName = SelectedDriver.DriverName;
            FormLicenseNumber = SelectedDriver.LicenseNumber;
            FormLicenseCategory = SelectedDriver.LicenseCategory;
            FormLicenseExpiry = SelectedDriver.LicenseExpiry;
            FormSafetyScore = SelectedDriver.SafetyScore;
            IsFormVisible = true;
        }

        private void ClearForm()
        {
            FormDriverName = string.Empty;
            FormLicenseNumber = string.Empty;
            FormLicenseCategory = "Van";
            FormLicenseExpiry = DateTime.Today.AddYears(1);
            FormSafetyScore = 100;
        }
    }
}
