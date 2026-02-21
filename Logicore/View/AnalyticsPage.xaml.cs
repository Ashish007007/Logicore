using Logicore.ViewModels;

namespace Logicore.View;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage(AnalyticsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AnalyticsViewModel vm)
            await vm.LoadAnalyticsAsync();
    }
}
