using Logicore.Enum;
using Logicore.Interfaces;
using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Text;
using Logicore.View;

namespace Logicore.Services;

public class NavigationService : INavigationervice
{
    private readonly IServiceProvider _serviceProvider;
    private NavigationPage? _navigationPage;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Window CreateMainWindow()
    {
        var loginPage = GetPage(PageKey.Login);

        _navigationPage = new NavigationPage(loginPage)
        {
            BarBackgroundColor = Colors.DarkBlue,
            BarTextColor = Colors.White
        };

        return new Window(_navigationPage);
    }

    public async Task NavigateToAsync(PageKey pageKey)
    {
        var page = GetPage(pageKey);

        if (_navigationPage != null)
            await _navigationPage.PushAsync(page, true);
    }

    public async Task GoBackAsync()
    {
        if (_navigationPage?.Navigation.NavigationStack.Count > 1)
            await _navigationPage.PopAsync(true);
    }

    public Task ShowAlertAsync(string title, string message, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
            Application.Current!.MainPage!.DisplayAlert(title, message, cancel));
    }

    public Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
            Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel));
    }

    private Page GetPage(PageKey key)
    {
        return key switch
        {
            PageKey.Login => _serviceProvider.GetRequiredService<LoginPage>(),
          //  PageKey.Register => _serviceProvider.GetRequiredService<>(),
            _ => throw new ArgumentOutOfRangeException(nameof(key))
        };
    }
}

