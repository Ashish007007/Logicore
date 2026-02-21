using Logicore.Models;
using Logicore.ViewModels;

namespace Logicore.View;

public partial class TripPage : ContentPage
{
    public TripPage(TripViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TripViewModel vm)
            await vm.LoadDataAsync();
    }

    // Handle vehicle picker selection — update FormVehicleId
    private void OnVehiclePickerChanged(object sender, EventArgs e)
    {
        if (BindingContext is TripViewModel vm && sender is Picker picker)
            if (picker.SelectedItem is Vehicle v)
                vm.FormVehicleId = v.VehicleId;
    }

    // Handle driver picker selection — update FormDriverId
    private void OnDriverPickerChanged(object sender, EventArgs e)
    {
        if (BindingContext is TripViewModel vm && sender is Picker picker)
            if (picker.SelectedItem is Driver d)
                vm.FormDriverId = d.DriverId;
    }

    private async void OnConfirmCompleteClicked(object sender, EventArgs e)
    {
        if (BindingContext is TripViewModel vm)
            await vm.SubmitCompletionAsync();
    }
}
