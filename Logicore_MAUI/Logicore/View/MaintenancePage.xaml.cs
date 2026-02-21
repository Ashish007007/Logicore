using Logicore.Models;
using Logicore.ViewModels;

namespace Logicore.View;

public partial class MaintenancePage : ContentPage
{
    public MaintenancePage(MaintenanceViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MaintenanceViewModel vm)
            await vm.LoadLogsAsync();
    }

    private void OnVehiclePickerChanged(object sender, EventArgs e)
    {
        if (BindingContext is MaintenanceViewModel vm && sender is Picker p)
            if (p.SelectedItem is Vehicle v)
                vm.FormVehicleId = v.VehicleId;
    }
}
