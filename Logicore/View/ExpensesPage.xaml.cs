using Logicore.Models;
using Logicore.ViewModels;

namespace Logicore.View;

public partial class ExpensesPage : ContentPage
{
    public ExpensesPage(ExpensesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ExpensesViewModel vm)
            await vm.LoadDataAsync();
    }

    private void OnFuelVehiclePickerChanged(object sender, EventArgs e)
    {
        if (BindingContext is ExpensesViewModel vm && sender is Picker p)
            if (p.SelectedItem is Vehicle v) vm.FormFuelVehicleId = v.VehicleId;
    }

    private void OnExpenseVehiclePickerChanged(object sender, EventArgs e)
    {
        if (BindingContext is ExpensesViewModel vm && sender is Picker p)
            if (p.SelectedItem is Vehicle v) vm.FormExpenseVehicleId = v.VehicleId;
    }
}
