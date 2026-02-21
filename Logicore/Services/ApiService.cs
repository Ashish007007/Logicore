using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Logicore.Models;

namespace Logicore.Services
{
    public class ApiService
    {
        // ── Change this to your Django server URL ──────────────────────────
        private const string BaseUrl = "http://127.0.0.1:8000/api/";
        // ──────────────────────────────────────────────────────────────────

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        // ─── AUTH ─────────────────────────────────────────────────────────
        public async Task<ApiResponse<LoginResponse>> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new LoginRequest { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("/login/", payload, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                    if (data != null)
                    {
                        SetAuthToken(data.Access);
                        await SecureStorage.Default.SetAsync("jwt_access", data.Access);
                        await SecureStorage.Default.SetAsync("jwt_refresh", data.Refresh);
                    }
                    return new ApiResponse<LoginResponse> { Success = true, Data = data };
                }
                var err = await response.Content.ReadAsStringAsync();
                return new ApiResponse<LoginResponse> { Success = false, Message = err };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse> { Success = false, Message = ex.Message };
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var refresh = await SecureStorage.Default.GetAsync("jwt_refresh");
                if (string.IsNullOrEmpty(refresh)) return false;

                var payload = new { refresh };
                var response = await _httpClient.PostAsJsonAsync("auth/token/refresh/", payload, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(_jsonOptions);
                    if (result != null && result.TryGetValue("access", out var newAccess))
                    {
                        SetAuthToken(newAccess);
                        await SecureStorage.Default.SetAsync("jwt_access", newAccess);
                        return true;
                    }
                }
            }
            catch { /* ignored */ }
            return false;
        }

        public async Task InitFromStorageAsync()
        {
            var token = await SecureStorage.Default.GetAsync("jwt_access");
            if (!string.IsNullOrEmpty(token))
                SetAuthToken(token);
        }

        public void SetAuthToken(string token) =>
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        public void Logout()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            SecureStorage.Default.Remove("jwt_access");
            SecureStorage.Default.Remove("jwt_refresh");
        }

        // ─── GENERIC HELPERS ─────────────────────────────────────────────
        private async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    if (await RefreshTokenAsync())
                        response = await _httpClient.GetAsync(endpoint);
                }
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                    return new ApiResponse<T> { Success = true, Data = data };
                }
                return new ApiResponse<T> { Success = false, Message = response.ReasonPhrase ?? "Error" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Message = ex.Message };
            }
        }

        private async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, body, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                    return new ApiResponse<TResponse> { Success = true, Data = data };
                }
                var err = await response.Content.ReadAsStringAsync();
                return new ApiResponse<TResponse> { Success = false, Message = err };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse> { Success = false, Message = ex.Message };
            }
        }

        private async Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            try
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
                    return new ApiResponse<TResponse> { Success = true, Data = data };
                }
                return new ApiResponse<TResponse> { Success = false, Message = response.ReasonPhrase ?? "Error" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse> { Success = false, Message = ex.Message };
            }
        }

        private async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ─── DASHBOARD ───────────────────────────────────────────────────
        public Task<ApiResponse<DashboardStats>> GetDashboardStatsAsync() =>
            GetAsync<DashboardStats>("dashboard/stats/");

        // ─── VEHICLES ────────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<Vehicle>>> GetVehiclesAsync(string? status = null)
        {
            var url = string.IsNullOrEmpty(status) ? "vehicles/" : $"vehicles/?status={status}";
            return GetAsync<PagedResponse<Vehicle>>(url);
        }

        public Task<ApiResponse<Vehicle>> GetVehicleAsync(int id) =>
            GetAsync<Vehicle>($"vehicles/{id}/");

        public Task<ApiResponse<Vehicle>> CreateVehicleAsync(Vehicle v) =>
            PostAsync<Vehicle, Vehicle>("vehicles/", v);

        public Task<ApiResponse<Vehicle>> UpdateVehicleAsync(int id, Vehicle v) =>
            PatchAsync<Vehicle, Vehicle>($"vehicles/{id}/", v);

        public Task<bool> DeleteVehicleAsync(int id) => DeleteAsync($"vehicles/{id}/");

        // ─── DRIVERS ─────────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<Driver>>> GetDriversAsync(string? status = null)
        {
            var url = string.IsNullOrEmpty(status) ? "drivers/" : $"drivers/?status={status}";
            return GetAsync<PagedResponse<Driver>>(url);
        }

        public Task<ApiResponse<Driver>> GetDriverAsync(int id) =>
            GetAsync<Driver>($"drivers/{id}/");

        public Task<ApiResponse<Driver>> CreateDriverAsync(Driver d) =>
            PostAsync<Driver, Driver>("drivers/", d);

        public Task<ApiResponse<Driver>> UpdateDriverAsync(int id, Driver d) =>
            PatchAsync<Driver, Driver>($"drivers/{id}/", d);

        // ─── TRIPS ───────────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<Trip>>> GetTripsAsync(string? status = null)
        {
            var url = string.IsNullOrEmpty(status) ? "trips/" : $"trips/?status={status}";
            return GetAsync<PagedResponse<Trip>>(url);
        }

        public Task<ApiResponse<Trip>> CreateTripAsync(TripCreateRequest t) =>
            PostAsync<TripCreateRequest, Trip>("trips/", t);

        public Task<ApiResponse<Trip>> UpdateTripStatusAsync(int id, string status) =>
            PatchAsync<object, Trip>($"trips/{id}/", new { status });

        public Task<ApiResponse<Trip>> CompleteTripAsync(int id, double endOdometer) =>
            PatchAsync<object, Trip>($"trips/{id}/complete/",
                new { end_odometer = endOdometer, status = "Completed" });

        // ─── MAINTENANCE ─────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<MaintenanceLog>>> GetMaintenanceLogsAsync(int? vehicleId = null)
        {
            var url = vehicleId.HasValue ? $"maintenance/?vehicle_id={vehicleId}" : "maintenance/";
            return GetAsync<PagedResponse<MaintenanceLog>>(url);
        }

        public Task<ApiResponse<MaintenanceLog>> CreateMaintenanceLogAsync(MaintenanceLog m) =>
            PostAsync<MaintenanceLog, MaintenanceLog>("maintenance/", m);

        // ─── FUEL LOGS ───────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<FuelLog>>> GetFuelLogsAsync(int? vehicleId = null)
        {
            var url = vehicleId.HasValue ? $"fuellogs/?vehicle_id={vehicleId}" : "fuellogs/";
            return GetAsync<PagedResponse<FuelLog>>(url);
        }

        public Task<ApiResponse<FuelLog>> CreateFuelLogAsync(FuelLog f) =>
            PostAsync<FuelLog, FuelLog>("fuellogs/", f);

        // ─── EXPENSES ────────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<Expense>>> GetExpensesAsync(int? vehicleId = null)
        {
            var url = vehicleId.HasValue ? $"expenses/?vehicle_id={vehicleId}" : "expenses/";
            return GetAsync<PagedResponse<Expense>>(url);
        }

        public Task<ApiResponse<Expense>> CreateExpenseAsync(Expense e) =>
            PostAsync<Expense, Expense>("expenses/", e);

        // ─── REVENUE ─────────────────────────────────────────────────────
        public Task<ApiResponse<PagedResponse<Revenue>>> GetRevenueAsync() =>
            GetAsync<PagedResponse<Revenue>>("revenue/");

        public Task<ApiResponse<Revenue>> CreateRevenueAsync(Revenue r) =>
            PostAsync<Revenue, Revenue>("revenue/", r);
    }
}
