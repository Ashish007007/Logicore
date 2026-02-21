using Logicore.ViewModels;

namespace Logicore.View;

public partial class VehiclePage : ContentPage
{
    public VehiclePage(VehicleViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VehicleViewModel vm)
            await vm.LoadVehiclesAsync();
    }
}
