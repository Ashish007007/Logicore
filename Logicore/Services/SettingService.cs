using Logicore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logicore.Services
{
    public class SettingService : IsettingService
    {
        #region Key Constants
            private const string IsLoggedInKey = "IsLoggedInKey";
        #endregion

        #region Fields
            public bool IsLoggedIn
            {
                get { return Preferences.Get(IsLoggedInKey, false); }
                set { Preferences.Set(IsLoggedInKey, value); }
            }
        #endregion
    }
}
