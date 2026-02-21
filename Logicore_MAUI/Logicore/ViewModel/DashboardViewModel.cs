using System.Collections.ObjectModel;
using Logicore.Models;
using Logicore.Services;

namespace Logicore.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ApiService _api;
       // private DashboardStats _stats = new();
        private string _userName = string.Empty;

        //public DashboardStats Stats
        //{
        //    get => _stats;
        //    set { SetProperty(ref _stats, value); }
        //}

        public string UserName
        {
            get => _userName;
            set { SetProperty(ref _userName, value); }
        }

        public ObservableCollection<Trip> RecentTrips { get; } = new();
        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand NavigateVehiclesCommand { get; }
        public RelayCommand NavigateTripsCommand { get; }
        public RelayCommand NavigateDriversCommand { get; }
        public RelayCommand NavigateMaintenanceCommand { get; }
        public RelayCommand NavigateExpensesCommand { get; }
        public RelayCommand NavigateAnalyticsCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public DashboardViewModel(ApiService api)
        {
            _api = api;
            Title = "Command Center";
            UserName = Preferences.Default.Get("user_name", "User");

            RefreshCommand = new AsyncRelayCommand(LoadDashboardAsync);
            NavigateVehiclesCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//vehicles"));
            NavigateTripsCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//trips"));
            NavigateDriversCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//drivers"));
            NavigateMaintenanceCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//maintenance"));
            NavigateExpensesCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//expenses"));
            NavigateAnalyticsCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//analytics"));
            LogoutCommand = new RelayCommand(DoLogout);
        }

        public async Task LoadDashboardAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                //var statsTask = _api.GetDashboardStatsAsync();
                //var tripsTask = _api.GetTripsAsync("Dispatched");
                //await Task.WhenAll(statsTask, tripsTask);

                //if (statsTask.Result.Success && statsTask.Result.Data != null)
                //    Stats = statsTask.Result.Data;

                //RecentTrips.Clear();
                //if (tripsTask.Result.Success && tripsTask.Result.Data != null)
                //    foreach (var t in tripsTask.Result.Data.Results.Take(5))
                //        RecentTrips.Add(t);
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            finally { IsBusy = false; }
        }

        private void DoLogout(object? _)
        {
         //   _api.Logout();
            Preferences.Default.Clear();
            Shell.Current.GoToAsync("//login");
        }
    }
}
