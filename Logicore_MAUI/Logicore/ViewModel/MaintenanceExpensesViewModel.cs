using System.Collections.ObjectModel;
using Logicore.Models;
using Logicore.Services;

namespace Logicore.ViewModels
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  MAINTENANCE VIEW MODEL
    // ═══════════════════════════════════════════════════════════════════════════
    public class MaintenanceViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private bool _isFormVisible;

        // Form fields
        private int _formVehicleId;
        private string _formServiceType = string.Empty;
        private DateTime _formServiceDate = DateTime.Today;
        private DateTime _formNextServiceDue = DateTime.Today.AddMonths(3);
        private double _formCost;
        private string _formRemarks = string.Empty;

        public ObservableCollection<MaintenanceLog> Logs { get; } = new();
        public ObservableCollection<Vehicle> AllVehicles { get; } = new();

        public bool IsFormVisible { get => _isFormVisible; set => SetProperty(ref _isFormVisible, value); }
        public int FormVehicleId { get => _formVehicleId; set => SetProperty(ref _formVehicleId, value); }
        public string FormServiceType { get => _formServiceType; set => SetProperty(ref _formServiceType, value); }
        public DateTime FormServiceDate { get => _formServiceDate; set => SetProperty(ref _formServiceDate, value); }
        public DateTime FormNextServiceDue { get => _formNextServiceDue; set => SetProperty(ref _formNextServiceDue, value); }
        public double FormCost { get => _formCost; set => SetProperty(ref _formCost, value); }
        public string FormRemarks { get => _formRemarks; set => SetProperty(ref _formRemarks, value); }

        public List<string> ServiceTypes { get; } = new()
            { "Oil Change", "Tire Replacement", "Brake Service", "Engine Check", "A/C Service", "Battery", "Other" };

        public AsyncRelayCommand LoadCommand { get; }
        public RelayCommand ShowAddFormCommand { get; }
        public RelayCommand HideFormCommand { get; }
        public AsyncRelayCommand SaveLogCommand { get; }

        public MaintenanceViewModel(ApiService api)
        {
            _api = api;
            Title = "Maintenance & Service Logs";
            LoadCommand = new AsyncRelayCommand(LoadLogsAsync);
            ShowAddFormCommand = new RelayCommand(async _ => await PrepareFormAsync());
            HideFormCommand = new RelayCommand(_ => { IsFormVisible = false; ClearForm(); });
            SaveLogCommand = new AsyncRelayCommand(SaveLogAsync);
        }

        public async Task LoadLogsAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _api.GetMaintenanceLogsAsync();
                Logs.Clear();
                if (result.Success && result.Data != null)
                    foreach (var l in result.Data.Results)
                        Logs.Add(l);
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task PrepareFormAsync()
        {
            var vResult = await _api.GetVehiclesAsync();
            AllVehicles.Clear();
            if (vResult.Success && vResult.Data != null)
                foreach (var v in vResult.Data.Results)
                    AllVehicles.Add(v);
            IsFormVisible = true;
        }

        private async Task SaveLogAsync()
        {
            if (FormVehicleId == 0 || string.IsNullOrWhiteSpace(FormServiceType))
            {
                ErrorMessage = "Vehicle and service type are required.";
                return;
            }
            IsBusy = true;
            ClearError();
            try
            {
                // Auto-set vehicle status to In Shop
                var vehicle = AllVehicles.FirstOrDefault(v => v.VehicleId == FormVehicleId);
                if (vehicle != null)
                    await _api.UpdateVehicleAsync(FormVehicleId, new Vehicle { Status = "In Shop" });

                var log = new MaintenanceLog
                {
                    VehicleId = FormVehicleId,
                    ServiceType = FormServiceType,
                    ServiceDate = FormServiceDate.ToString(),
                    NextServiceDue = FormNextServiceDue.ToString(),
                    Cost = FormCost,
                    Remarks = FormRemarks
                };
                var result = await _api.CreateMaintenanceLogAsync(log);
                if (result.Success)
                {
                    IsFormVisible = false;
                    ClearForm();
                    await LoadLogsAsync();
                    await Shell.Current.DisplayAlert("Vehicle In Shop",
                        $"Maintenance logged. {vehicle?.VehicleName} status set to 'In Shop'.", "OK");
                }
                else
                    ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private void ClearForm()
        {
            FormVehicleId = 0;
            FormServiceType = string.Empty;
            FormServiceDate = DateTime.Today;
            FormNextServiceDue = DateTime.Today.AddMonths(3);
            FormCost = 0;
            FormRemarks = string.Empty;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  EXPENSES VIEW MODEL
    // ═══════════════════════════════════════════════════════════════════════════
    public class ExpensesViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private bool _isFuelFormVisible;
        private bool _isExpenseFormVisible;

        // Fuel form
        private int _formFuelVehicleId;
        private double _formLiters;
        private double _formFuelCost;
        private DateTime _formFuelDate = DateTime.Today;

        // Expense form
        private int _formExpenseVehicleId;
        private string _formExpenseType = string.Empty;
        private double _formExpenseAmount;
        private DateTime _formExpenseDate = DateTime.Today;
        private string _formExpenseRemarks = string.Empty;

        public ObservableCollection<FuelLog> FuelLogs { get; } = new();
        public ObservableCollection<Expense> Expenses { get; } = new();
        public ObservableCollection<Vehicle> AllVehicles { get; } = new();

        public bool IsFuelFormVisible { get => _isFuelFormVisible; set => SetProperty(ref _isFuelFormVisible, value); }
        public bool IsExpenseFormVisible { get => _isExpenseFormVisible; set => SetProperty(ref _isExpenseFormVisible, value); }

        public int FormFuelVehicleId { get => _formFuelVehicleId; set => SetProperty(ref _formFuelVehicleId, value); }
        public double FormLiters { get => _formLiters; set => SetProperty(ref _formLiters, value); }
        public double FormFuelCost { get => _formFuelCost; set => SetProperty(ref _formFuelCost, value); }
        public DateTime FormFuelDate { get => _formFuelDate; set => SetProperty(ref _formFuelDate, value); }

        public int FormExpenseVehicleId { get => _formExpenseVehicleId; set => SetProperty(ref _formExpenseVehicleId, value); }
        public string FormExpenseType { get => _formExpenseType; set => SetProperty(ref _formExpenseType, value); }
        public double FormExpenseAmount { get => _formExpenseAmount; set => SetProperty(ref _formExpenseAmount, value); }
        public DateTime FormExpenseDate { get => _formExpenseDate; set => SetProperty(ref _formExpenseDate, value); }
        public string FormExpenseRemarks { get => _formExpenseRemarks; set => SetProperty(ref _formExpenseRemarks, value); }

        public List<string> ExpenseTypes { get; } = new()
            { "Fuel", "Toll", "Parking", "Repair", "Insurance", "Registration", "Other" };

        public AsyncRelayCommand LoadCommand { get; }
        public RelayCommand ShowFuelFormCommand { get; }
        public RelayCommand HideFuelFormCommand { get; }
        public AsyncRelayCommand SaveFuelLogCommand { get; }
        public RelayCommand ShowExpenseFormCommand { get; }
        public RelayCommand HideExpenseFormCommand { get; }
        public AsyncRelayCommand SaveExpenseCommand { get; }

        public ExpensesViewModel(ApiService api)
        {
            _api = api;
            Title = "Fuel & Expenses";
            LoadCommand = new AsyncRelayCommand(LoadDataAsync);
            ShowFuelFormCommand = new RelayCommand(async _ => await PrepareVehiclesAsync(true));
            HideFuelFormCommand = new RelayCommand(_ => IsFuelFormVisible = false);
            SaveFuelLogCommand = new AsyncRelayCommand(SaveFuelLogAsync);
            ShowExpenseFormCommand = new RelayCommand(async _ => await PrepareVehiclesAsync(false));
            HideExpenseFormCommand = new RelayCommand(_ => IsExpenseFormVisible = false);
            SaveExpenseCommand = new AsyncRelayCommand(SaveExpenseAsync);
        }

        public async Task LoadDataAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var fuelTask = _api.GetFuelLogsAsync();
                var expTask = _api.GetExpensesAsync();
                await Task.WhenAll(fuelTask, expTask);

                FuelLogs.Clear();
                if (fuelTask.Result.Success && fuelTask.Result.Data != null)
                    foreach (var f in fuelTask.Result.Data.Results)
                        FuelLogs.Add(f);

                Expenses.Clear();
                if (expTask.Result.Success && expTask.Result.Data != null)
                    foreach (var e in expTask.Result.Data.Results)
                        Expenses.Add(e);
            }
            finally { IsBusy = false; }
        }

        private async Task PrepareVehiclesAsync(bool isFuel)
        {
            var result = await _api.GetVehiclesAsync();
            AllVehicles.Clear();
            if (result.Success && result.Data != null)
                foreach (var v in result.Data.Results)
                    AllVehicles.Add(v);

            if (isFuel) IsFuelFormVisible = true;
            else IsExpenseFormVisible = true;
        }

        private async Task SaveFuelLogAsync()
        {
            if (FormFuelVehicleId == 0 || FormLiters <= 0)
            {
                ErrorMessage = "Vehicle and liters are required.";
                return;
            }
            IsBusy = true;
            ClearError();
            try
            {
                var log = new FuelLog
                {
                    VehicleId = FormFuelVehicleId,
                    Liters = FormLiters,
                    Cost = FormFuelCost,
                    FuelDate = FormFuelDate.ToString()
                };
                var result = await _api.CreateFuelLogAsync(log);
                if (result.Success) { IsFuelFormVisible = false; await LoadDataAsync(); }
                else ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task SaveExpenseAsync()
        {
            if (FormExpenseVehicleId == 0 || string.IsNullOrWhiteSpace(FormExpenseType))
            {
                ErrorMessage = "Vehicle and expense type are required.";
                return;
            }
            IsBusy = true;
            ClearError();
            try
            {
                var expense = new Expense
                {
                    VehicleId = FormExpenseVehicleId,
                    ExpenseType = FormExpenseType,
                    Amount = FormExpenseAmount,
                    ExpenseDate = FormExpenseDate.ToString(),
                    Remarks = FormExpenseRemarks
                };
                var result = await _api.CreateExpenseAsync(expense);
                if (result.Success) { IsExpenseFormVisible = false; await LoadDataAsync(); }
                else ErrorMessage = result.Message;
            }
            finally { IsBusy = false; }
        }
    }
}
