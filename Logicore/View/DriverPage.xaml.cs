using Logicore.Models;
using Logicore.ViewModels;

namespace Logicore.View;

public partial class DriverPage : ContentPage
{
    public DriverPage(DriverViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DriverViewModel vm)
            await vm.LoadDriversAsync();
    }
}
