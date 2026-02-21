using Logicore.Interfaces;
using Logicore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logicore
{
    public partial class App : Application
    {
        private readonly ApiService _apiService;

        public App(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            // Register global converters
            Resources.MergedDictionaries.Add(new ResourceDictionary());
            RegisterConverters();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            // Try restore JWT from secure storage
            await _apiService.InitFromStorageAsync();

            var token = await SecureStorage.Default.GetAsync("jwt_access");
            if (!string.IsNullOrEmpty(token))
                await Shell.Current.GoToAsync("//dashboard");
            else
                await Shell.Current.GoToAsync("//login");
        }

        private void RegisterConverters()
        {
           // Resources.Add("StatusColorConverter", new Converters.StatusColorConverter());
            //Resources.Add("BusyTextConverter", new Converters.BusyTextConverter());
            //Resources.Add("NotNullConverter", new Converters.NotNullConverter());
            //Resources.Add("StatusVisibleConverter", new Converters.StatusVisibleConverter());
            //Resources.Add("InverseBoolConverter", new Converters.InverseBoolConverter());
        }
    }
}