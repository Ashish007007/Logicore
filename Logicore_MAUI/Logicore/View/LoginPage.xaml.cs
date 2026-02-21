using Logicore.ViewModels;

namespace Logicore.View;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Clear any stale error on page appear
        if (BindingContext is LoginViewModel vm)
            vm.ErrorMessage = string.Empty;
    }
}
