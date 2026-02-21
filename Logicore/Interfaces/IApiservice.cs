using System;
using System.Collections.Generic;
using System.Text;

namespace Logicore.Interfaces
{
    public interface IApiservice
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<TRequest, T>(string endpoint, TRequest data);
    }
}
