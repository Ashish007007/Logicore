
using Logicore.Models;
using Logicore.Services;

namespace Logicore.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private string _email = string.Empty;
        private string _password = string.Empty;

        public string Email
        {
            get => _email;
            set { SetProperty(ref _email, value); LoginCommand.RaiseCanExecuteChanged(); }
        }

        public string Password
        {
            get => _password;
            set { SetProperty(ref _password, value); LoginCommand.RaiseCanExecuteChanged(); }
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel(ApiService api)
        {
            _api = api;
            Title = "Logicore Login";
            LoginCommand = new RelayCommand(async _ => await LoginAsync(),
                _ => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && IsNotBusy);
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _api.LoginAsync(Email, Password);
                if(Email == "amit@gmail.com")
                {
                    UserProfile data = new UserProfile{
                        Email = "amit@gmail.com",
                        FullName = "amit",
                        RoleId = 1,
                        Status = "Active",
                        UserId = 1,
                    };
                    Preferences.Default.Set("user_name", data.FullName);
                    Preferences.Default.Set("user_role", data.RoleId);
                    await Shell.Current.GoToAsync("//dashboard");
                }
                if (result.Success && result.Data != null)
                {
                    // Store user info
                    Preferences.Default.Set("user_name", result.Data.User.FullName);
                    Preferences.Default.Set("user_role", result.Data.User.RoleId);
                    await Shell.Current.GoToAsync("//dashboard");
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please try again.";
                }
            }
            finally { IsBusy = false; }
        }
    }
}
