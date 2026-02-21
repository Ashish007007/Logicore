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

        // Django POST /api/login/  →  body: { email, password }
        // Response: { user_id, role_id, full_name, email, status, created_at }
        // There is NO JWT — no tokens, no refresh, no Bearer header needed.
        public async Task<ApiResponse<LoginResponse>> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new LoginRequest { Email = email, Password = password };
                // FIX: was "/login/" (absolute, escapes BaseAddress) → "login/" (relative)
                var response = await _httpClient.PostAsJsonAsync("login/", payload, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                    // Store user info for later use in the session
                    if (data != null)
                    {
                        await SecureStorage.Default.SetAsync("user_id", data.UserId.ToString());
                        await SecureStorage.Default.SetAsync("role_id", data.RoleId.ToString());
                        await SecureStorage.Default.SetAsync("full_name", data.FullName);
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

        // Django has no token-refresh endpoint — this is a no-op stub kept so
        // existing code that calls RefreshTokenAsync() still compiles.
        public Task<bool> RefreshTokenAsync() => Task.FromResult(false);

        // No JWT, so InitFromStorageAsync is a no-op stub.
        public Task InitFromStorageAsync() => Task.CompletedTask;

        // No JWT, so SetAuthToken is a no-op stub.
        public void SetAuthToken(string token) { }

        // Clears the stored session data.
        public void Logout()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            SecureStorage.Default.Remove("user_id");
            SecureStorage.Default.Remove("role_id");
            SecureStorage.Default.Remove("full_name");
        }

        // ─── GENERIC HELPERS ─────────────────────────────────────────────

        private async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
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

        // PatchAsync and DeleteAsync are kept so existing code still compiles,
        // but Django does not use PATCH or DELETE — every mutation is POST.
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
        // No dashboard endpoint exists in Django yet — returns empty stats.
        public Task<ApiResponse<DashboardStats>> GetDashboardStatsAsync() =>
            GetAsync<DashboardStats>("dashboard/stats/");

        // ─── VEHICLES ────────────────────────────────────────────────────

        // GET /api/vehicles/all/  →  { "vehicles": [...] }
        // FIX: was GET "vehicles/" + PagedResponse<Vehicle> (doesn't exist in Django)
        public async Task<ApiResponse<PagedResponse<Vehicle>>> GetVehiclesAsync(string? status = null)
        {
            var result = await GetAsync<VehicleListResponse>("vehicles/all/");
            if (!result.Success)
                return new ApiResponse<PagedResponse<Vehicle>> { Success = false, Message = result.Message };

            var list = result.Data?.Vehicles ?? new List<Vehicle>();
            var paged = new PagedResponse<Vehicle> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Vehicle>> { Success = true, Data = paged };
        }

        // POST /api/vehicles/get/  →  body: { vehicle_id }
        // FIX: was GET "vehicles/{id}/"
        public async Task<ApiResponse<Vehicle>> GetVehicleAsync(int id)
        {
            var payload = new { vehicle_id = id };
            return await PostAsync<object, Vehicle>("vehicles/get/", payload);
        }

        // POST /api/vehicles/create/  →  body: full vehicle fields
        // FIX: was POST "vehicles/" — wrong URL
        public async Task<ApiResponse<Vehicle>> CreateVehicleAsync(Vehicle v)
        {
            var payload = new
            {
                vehicle_id = v.VehicleId,
                vehicle_name = v.VehicleName,
                vehicle_type = v.VehicleType,
                license_plate = v.LicensePlate,
                max_capacity_kg = v.MaxCapacityKg,
                odometer = v.Odometer,
                acquisition_cost = v.AcquisitionCost,
                status = v.Status
            };
            var result = await PostAsync<object, MessageResponse>("vehicles/create/", payload);
            // Return the original vehicle object on success (Django only returns message + id)
            return new ApiResponse<Vehicle>
            {
                Success = result.Success,
                Data = result.Success ? v : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/vehicles/update/  →  body: { vehicle_id, ...fields }
        // FIX: was PATCH "vehicles/{id}/"
        public async Task<ApiResponse<Vehicle>> UpdateVehicleAsync(int id, Vehicle v)
        {
            v.VehicleId = id;
            var payload = new
            {
                vehicle_id = v.VehicleId,
                vehicle_name = v.VehicleName,
                vehicle_type = v.VehicleType,
                license_plate = v.LicensePlate,
                max_capacity_kg = v.MaxCapacityKg,
                odometer = v.Odometer,
                acquisition_cost = v.AcquisitionCost,
                status = v.Status
            };
            var result = await PostAsync<object, MessageResponse>("vehicles/update/", payload);
            return new ApiResponse<Vehicle>
            {
                Success = result.Success,
                Data = result.Success ? v : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/vehicles/delete/  →  body: { vehicle_id }
        // FIX: was DELETE "vehicles/{id}/"
        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var payload = new { vehicle_id = id };
            var result = await PostAsync<object, MessageResponse>("vehicles/delete/", payload);
            return result.Success;
        }

        // POST /api/vehicles/deactivate/  →  body: { vehicle_id }
        // FIX: was missing entirely in the original ApiService
        public async Task<bool> DeactivateVehicleAsync(int id)
        {
            var payload = new { vehicle_id = id };
            var result = await PostAsync<object, MessageResponse>("vehicles/deactivate/", payload);
            return result.Success;
        }

        // ─── DRIVERS ─────────────────────────────────────────────────────

        // GET /api/drivers/all/  →  { "drivers": [...] }
        // FIX: was GET "drivers/" + PagedResponse<Driver>
        public async Task<ApiResponse<PagedResponse<Driver>>> GetDriversAsync(string? status = null)
        {
            var result = await GetAsync<DriverListResponse>("drivers/all/");
            if (!result.Success)
                return new ApiResponse<PagedResponse<Driver>> { Success = false, Message = result.Message };

            var list = result.Data?.Drivers ?? new List<Driver>();
            var paged = new PagedResponse<Driver> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Driver>> { Success = true, Data = paged };
        }

        // POST /api/drivers/get/  →  body: { driver_id }
        // FIX: was GET "drivers/{id}/"
        public async Task<ApiResponse<Driver>> GetDriverAsync(int id)
        {
            var payload = new { driver_id = id };
            return await PostAsync<object, Driver>("drivers/get/", payload);
        }

        // POST /api/drivers/create/  →  body: full driver fields
        // FIX: was POST "drivers/" — wrong URL; also LicenseExpiry must be string "YYYY-MM-DD"
        public async Task<ApiResponse<Driver>> CreateDriverAsync(Driver d)
        {
            var payload = new
            {
                driver_id = d.DriverId,
                driver_name = d.DriverName,
                license_number = d.LicenseNumber,
                license_category = d.LicenseCategory,
                license_expiry = d.LicenseExpiry,  // already "YYYY-MM-DD" string in model
                safety_score = d.SafetyScore,
                status = d.Status
            };
            var result = await PostAsync<object, MessageResponse>("drivers/create/", payload);
            return new ApiResponse<Driver>
            {
                Success = result.Success,
                Data = result.Success ? d : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/drivers/update/  →  body: { driver_id, driver_name?, license_category?, status?, safety_score? }
        // FIX: was PATCH "drivers/{id}/"
        public async Task<ApiResponse<Driver>> UpdateDriverAsync(int id, Driver d)
        {
            d.DriverId = id;
            var payload = new
            {
                driver_id = d.DriverId,
                driver_name = d.DriverName,
                license_category = d.LicenseCategory,
                safety_score = d.SafetyScore,
                status = d.Status
            };
            var result = await PostAsync<object, MessageResponse>("drivers/update/", payload);
            return new ApiResponse<Driver>
            {
                Success = result.Success,
                Data = result.Success ? d : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/drivers/deactivate/  →  body: { driver_id }
        // FIX: was missing entirely in the original ApiService
        public async Task<bool> DeactivateDriverAsync(int id)
        {
            var payload = new { driver_id = id };
            var result = await PostAsync<object, MessageResponse>("drivers/deactivate/", payload);
            return result.Success;
        }

        // ─── TRIPS ───────────────────────────────────────────────────────

        // GET /api/trips/all/  →  { "trips": [...] }
        // FIX: was GET "trips/" + PagedResponse<Trip>
        public async Task<ApiResponse<PagedResponse<Trip>>> GetTripsAsync(string? status = null)
        {
            var result = await GetAsync<TripListResponse>("trips/all/");
            if (!result.Success)
                return new ApiResponse<PagedResponse<Trip>> { Success = false, Message = result.Message };

            var list = result.Data?.Trips ?? new List<Trip>();
            var paged = new PagedResponse<Trip> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Trip>> { Success = true, Data = paged };
        }

        // POST /api/trips/create/  →  body: { trip_id, vehicle_id, driver_id, cargo_weight,
        //                                      origin, destination, start_date, start_odometer }
        // FIX: was POST "trips/" — wrong URL; also TripCreateRequest now requires TripId
        public async Task<ApiResponse<Trip>> CreateTripAsync(TripCreateRequest t)
        {
            var payload = new
            {
                trip_id = t.TripId,
                vehicle_id = t.VehicleId,
                driver_id = t.DriverId,
                cargo_weight = t.CargoWeight,
                origin = t.Origin,
                destination = t.Destination,
                start_date = t.StartDate.ToString("yyyy-MM-dd"),
                start_odometer = t.StartOdometer
            };
            var result = await PostAsync<object, MessageResponse>("trips/create/", payload);
            return new ApiResponse<Trip>
            {
                Success = result.Success,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/trips/get/  →  body: { trip_id }
        // FIX: was missing in the original ApiService
        public async Task<ApiResponse<Trip>> GetTripAsync(int id)
        {
            var payload = new { trip_id = id };
            return await PostAsync<object, Trip>("trips/get/", payload);
        }

        // UpdateTripStatusAsync — Django has no generic status-update endpoint.
        // Kept as a stub so existing code compiles; use CompleteTripAsync instead.
        public Task<ApiResponse<Trip>> UpdateTripStatusAsync(int id, string status) =>
            Task.FromResult(new ApiResponse<Trip>
            {
                Success = false,
                Message = "Use CompleteTripAsync to complete a trip. Django has no generic status update endpoint."
            });

        // POST /api/trips/complete/  →  body: { trip_id, end_odometer }
        // FIX: was PATCH "trips/{id}/complete/"
        public async Task<ApiResponse<Trip>> CompleteTripAsync(int id, double endOdometer)
        {
            var payload = new { trip_id = id, end_odometer = endOdometer };
            var result = await PostAsync<object, MessageResponse>("trips/complete/", payload);
            return new ApiResponse<Trip>
            {
                Success = result.Success,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // ─── MAINTENANCE ─────────────────────────────────────────────────

        // GET /api/maintenance/all/  →  { "maintenance_logs": [...] }
        // POST /api/maintenance/by-vehicle/  →  body: { vehicle_id }
        // FIX: was GET "maintenance/" + optional query param — doesn't exist in Django
        public async Task<ApiResponse<PagedResponse<MaintenanceLog>>> GetMaintenanceLogsAsync(int? vehicleId = null)
        {
            List<MaintenanceLog> list;

            if (vehicleId.HasValue)
            {
                var payload = new { vehicle_id = vehicleId.Value };
                var result = await PostAsync<object, MaintenanceListResponse>("maintenance/by-vehicle/", payload);
                if (!result.Success)
                    return new ApiResponse<PagedResponse<MaintenanceLog>> { Success = false, Message = result.Message };
                list = result.Data?.MaintenanceLogs ?? new List<MaintenanceLog>();
            }
            else
            {
                var result = await GetAsync<MaintenanceListResponse>("maintenance/all/");
                if (!result.Success)
                    return new ApiResponse<PagedResponse<MaintenanceLog>> { Success = false, Message = result.Message };
                list = result.Data?.MaintenanceLogs ?? new List<MaintenanceLog>();
            }

            var paged = new PagedResponse<MaintenanceLog> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<MaintenanceLog>> { Success = true, Data = paged };
        }

        // POST /api/maintenance/create/  →  body: full maintenance fields
        // FIX: was POST "maintenance/" — wrong URL; dates must be "YYYY-MM-DD" strings
        public async Task<ApiResponse<MaintenanceLog>> CreateMaintenanceLogAsync(MaintenanceLog m)
        {
            var payload = new
            {
                maintenance_id = m.MaintenanceId,
                vehicle_id = m.VehicleId,
                service_type = m.ServiceType,
                service_date = m.ServiceDate,     // already "YYYY-MM-DD" string in model
                next_service_due = m.NextServiceDue,  // already "YYYY-MM-DD" string in model
                cost = m.Cost,
                remarks = m.Remarks
            };
            var result = await PostAsync<object, MessageResponse>("maintenance/create/", payload);
            return new ApiResponse<MaintenanceLog>
            {
                Success = result.Success,
                Data = result.Success ? m : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/maintenance/complete/  →  body: { vehicle_id }
        // Sets vehicle status back to "Available"
        // FIX: was missing entirely in the original ApiService
        public async Task<bool> CompleteMaintenanceAsync(int vehicleId)
        {
            var payload = new { vehicle_id = vehicleId };
            var result = await PostAsync<object, MessageResponse>("maintenance/complete/", payload);
            return result.Success;
        }

        // ─── FUEL LOGS ───────────────────────────────────────────────────

        // GET /api/fuel/all/  →  { "fuel_logs": [...] }
        // POST /api/fuel/by-vehicle/  →  body: { vehicle_id }
        // FIX: was GET "fuellogs/" — wrong URL
        public async Task<ApiResponse<PagedResponse<FuelLog>>> GetFuelLogsAsync(int? vehicleId = null)
        {
            List<FuelLog> list;

            if (vehicleId.HasValue)
            {
                var payload = new { vehicle_id = vehicleId.Value };
                var result = await PostAsync<object, FuelLogListResponse>("fuel/by-vehicle/", payload);
                if (!result.Success)
                    return new ApiResponse<PagedResponse<FuelLog>> { Success = false, Message = result.Message };
                list = result.Data?.FuelLogs ?? new List<FuelLog>();
            }
            else
            {
                var result = await GetAsync<FuelLogListResponse>("fuel/all/");
                if (!result.Success)
                    return new ApiResponse<PagedResponse<FuelLog>> { Success = false, Message = result.Message };
                list = result.Data?.FuelLogs ?? new List<FuelLog>();
            }

            var paged = new PagedResponse<FuelLog> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<FuelLog>> { Success = true, Data = paged };
        }

        // POST /api/fuel/create/  →  body: { fuel_id, vehicle_id, trip_id, liters, cost, fuel_date }
        // FIX: was POST "fuellogs/" — wrong URL
        public async Task<ApiResponse<FuelLog>> CreateFuelLogAsync(FuelLog f)
        {
            var payload = new
            {
                fuel_id = f.FuelId,
                vehicle_id = f.VehicleId,
                trip_id = f.TripId,
                liters = f.Liters,
                cost = f.Cost,
                fuel_date = f.FuelDate   // already "YYYY-MM-DD" string in model
            };
            var result = await PostAsync<object, MessageResponse>("fuel/create/", payload);
            return new ApiResponse<FuelLog>
            {
                Success = result.Success,
                Data = result.Success ? f : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/fuel/efficiency/  →  body: { trip_id }
        // Returns: { trip_id, distance_km, total_liters, fuel_efficiency_km_per_liter }
        // FIX: was missing entirely in the original ApiService
        public Task<ApiResponse<FuelEfficiencyResponse>> CalculateFuelEfficiencyAsync(int tripId)
        {
            var payload = new { trip_id = tripId };
            return PostAsync<object, FuelEfficiencyResponse>("fuel/efficiency/", payload);
        }

        // ─── EXPENSES ────────────────────────────────────────────────────

        // GET /api/expenses/all/  →  { "expenses": [...] }
        // POST /api/expenses/by-vehicle/  →  body: { vehicle_id }
        // FIX: was GET "expenses/" — wrong URL
        public async Task<ApiResponse<PagedResponse<Expense>>> GetExpensesAsync(int? vehicleId = null)
        {
            List<Expense> list;

            if (vehicleId.HasValue)
            {
                var payload = new { vehicle_id = vehicleId.Value };
                var result = await PostAsync<object, ExpenseListResponse>("expenses/by-vehicle/", payload);
                if (!result.Success)
                    return new ApiResponse<PagedResponse<Expense>> { Success = false, Message = result.Message };
                list = result.Data?.Expenses ?? new List<Expense>();
            }
            else
            {
                var result = await GetAsync<ExpenseListResponse>("expenses/all/");
                if (!result.Success)
                    return new ApiResponse<PagedResponse<Expense>> { Success = false, Message = result.Message };
                list = result.Data?.Expenses ?? new List<Expense>();
            }

            var paged = new PagedResponse<Expense> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Expense>> { Success = true, Data = paged };
        }

        // POST /api/expenses/create/  →  body: { expense_id, vehicle_id, expense_type, amount, expense_date, remarks }
        // FIX: was POST "expenses/" — wrong URL
        public async Task<ApiResponse<Expense>> CreateExpenseAsync(Expense e)
        {
            var payload = new
            {
                expense_id = e.ExpenseId,
                vehicle_id = e.VehicleId,
                expense_type = e.ExpenseType,
                amount = e.Amount,
                expense_date = e.ExpenseDate,  // already "YYYY-MM-DD" string in model
                remarks = e.Remarks
            };
            var result = await PostAsync<object, MessageResponse>("expenses/create/", payload);
            return new ApiResponse<Expense>
            {
                Success = result.Success,
                Data = result.Success ? e : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/expenses/total/  →  body: { vehicle_id }
        // Returns: { vehicle_id, total_expense }
        // FIX: was missing entirely in the original ApiService
        public Task<ApiResponse<TotalExpenseResponse>> CalculateTotalExpenseAsync(int vehicleId)
        {
            var payload = new { vehicle_id = vehicleId };
            return PostAsync<object, TotalExpenseResponse>("expenses/total/", payload);
        }

        // ─── REVENUE ─────────────────────────────────────────────────────

        // GET /api/revenue/all/  →  { "revenues": [...] }
        // FIX: was GET "revenue/" + PagedResponse<Revenue>
        public async Task<ApiResponse<PagedResponse<Revenue>>> GetRevenueAsync()
        {
            var result = await GetAsync<RevenueListResponse>("revenue/all/");
            if (!result.Success)
                return new ApiResponse<PagedResponse<Revenue>> { Success = false, Message = result.Message };

            var list = result.Data?.Revenues ?? new List<Revenue>();
            var paged = new PagedResponse<Revenue> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Revenue>> { Success = true, Data = paged };
        }

        // POST /api/revenue/create/  →  body: { revenue_id, trip_id, revenue_amount, received_date }
        // FIX: was POST "revenue/" — wrong URL
        public async Task<ApiResponse<Revenue>> CreateRevenueAsync(Revenue r)
        {
            var payload = new
            {
                revenue_id = r.RevenueId,
                trip_id = r.TripId,
                revenue_amount = r.RevenueAmount,
                received_date = r.ReceivedDate   // already "YYYY-MM-DD" string in model
            };
            var result = await PostAsync<object, MessageResponse>("revenue/create/", payload);
            return new ApiResponse<Revenue>
            {
                Success = result.Success,
                Data = result.Success ? r : null,
                Message = result.Data?.Message ?? result.Message
            };
        }

        // POST /api/revenue/by-trip/  →  body: { trip_id }
        // FIX: was missing entirely in the original ApiService
        public async Task<ApiResponse<PagedResponse<Revenue>>> GetRevenueByTripAsync(int tripId)
        {
            var payload = new { trip_id = tripId };
            var result = await PostAsync<object, RevenueListResponse>("revenue/by-trip/", payload);
            if (!result.Success)
                return new ApiResponse<PagedResponse<Revenue>> { Success = false, Message = result.Message };

            var list = result.Data?.Revenues ?? new List<Revenue>();
            var paged = new PagedResponse<Revenue> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Revenue>> { Success = true, Data = paged };
        }

        // POST /api/revenue/roi/  →  body: { vehicle_id }
        // Returns: { vehicle_id, total_revenue, total_expense, roi }
        // FIX: was missing entirely in the original ApiService
        public Task<ApiResponse<RoiResponse>> CalculateRoiAsync(int vehicleId)
        {
            var payload = new { vehicle_id = vehicleId };
            return PostAsync<object, RoiResponse>("revenue/roi/", payload);
        }

        // ─── ROLES ───────────────────────────────────────────────────────
        // These endpoints exist in Django but were missing from the original ApiService.

        // GET /api/roles/all/  →  { "roles": [...] }
        public async Task<ApiResponse<PagedResponse<Role>>> GetRolesAsync()
        {
            var result = await GetAsync<RoleListResponse>("roles/all/");
            if (!result.Success)
                return new ApiResponse<PagedResponse<Role>> { Success = false, Message = result.Message };

            var list = result.Data?.Roles ?? new List<Role>();
            var paged = new PagedResponse<Role> { Count = list.Count, Results = list };
            return new ApiResponse<PagedResponse<Role>> { Success = true, Data = paged };
        }

        // POST /api/roles/create/  →  body: { role_id, role_name, description }
        public async Task<bool> CreateRoleAsync(Role role)
        {
            var payload = new { role_id = role.RoleId, role_name = role.RoleName, description = role.Description };
            var result = await PostAsync<object, MessageResponse>("roles/create/", payload);
            return result.Success;
        }

        // POST /api/roles/get/  →  body: { role_id }
        public Task<ApiResponse<Role>> GetRoleAsync(int roleId)
        {
            var payload = new { role_id = roleId };
            return PostAsync<object, Role>("roles/get/", payload);
        }

        // POST /api/roles/update/  →  body: { role_id, role_name?, description? }
        public async Task<bool> UpdateRoleAsync(Role role)
        {
            var payload = new { role_id = role.RoleId, role_name = role.RoleName, description = role.Description };
            var result = await PostAsync<object, MessageResponse>("roles/update/", payload);
            return result.Success;
        }

        // POST /api/roles/delete/  →  body: { role_id }
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var payload = new { role_id = roleId };
            var result = await PostAsync<object, MessageResponse>("roles/delete/", payload);
            return result.Success;
        }

        // ─── USERS ───────────────────────────────────────────────────────
        // These endpoints exist in Django but were missing from the original ApiService.

        // POST /api/register/  →  body: { role_id, full_name, email, password, status }
        public async Task<bool> RegisterUserAsync(int roleId, string fullName, string email, string password, string status = "Active")
        {
            var payload = new { role_id = roleId, full_name = fullName, email, password, status };
            var result = await PostAsync<object, MessageResponse>("register/", payload);
            return result.Success;
        }

        // POST /api/get-user/  →  body: { user_id }
        public Task<ApiResponse<UserProfile>> GetUserAsync(int userId)
        {
            var payload = new { user_id = userId };
            return PostAsync<object, UserProfile>("get-user/", payload);
        }
    }
}
