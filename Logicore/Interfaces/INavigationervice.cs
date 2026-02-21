using Logicore.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logicore.Interfaces;

public interface INavigationervice
{
        Window CreateMainWindow();

        Task NavigateToAsync(PageKey pageKey);
        Task GoBackAsync();

        Task ShowAlertAsync(string title, string message, string cancel);
        Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);
}
