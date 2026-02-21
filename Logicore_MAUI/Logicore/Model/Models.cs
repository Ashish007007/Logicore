using System;
using System.Collections.Generic;

namespace Logicore.Models
{
    // ─── AUTH ───────────────────────────────────────────────────────────────
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Django /login/ returns the user fields directly (no JWT tokens).
    // Access/Refresh are kept as empty strings so any existing code that
    // references them still compiles. The User property maps to UserProfile
    // so old code using data.User.X continues to work.
    public class LoginResponse
    {
        public string Access { get; set; } = string.Empty;   // kept for compile compatibility
        public string Refresh { get; set; } = string.Empty;  // kept for compile compatibility
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;

        // Shim so existing code using loginResponse.User.FullName still compiles
        public UserProfile User => new UserProfile
        {
            UserId = UserId,
            RoleId = RoleId,
            FullName = FullName,
            Email = Email,
            Status = Status,
            CreatedAt = CreatedAt
        };
    }

    public class UserProfile
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    // ─── VEHICLE ────────────────────────────────────────────────────────────
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public double MaxCapacityKg { get; set; }
        public double Odometer { get; set; }
        public double AcquisitionCost { get; set; }
        public string Status { get; set; } = "Available"; // Available | On Trip | In Shop | Inactive
        // Django returns dates as "YYYY-MM-DD" strings, not DateTime objects
        public string CreatedAt { get; set; } = string.Empty;
    }

    // ─── DRIVER ─────────────────────────────────────────────────────────────
    public class Driver
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseCategory { get; set; } = string.Empty;
        // Django returns dates as "YYYY-MM-DD" strings
        public string LicenseExpiry { get; set; } = string.Empty;
        public double SafetyScore { get; set; }
        public string Status { get; set; } = "Off Duty"; // On Duty | Off Duty | On Trip | Inactive
        public string CreatedAt { get; set; } = string.Empty;
    }

    // ─── TRIP ───────────────────────────────────────────────────────────────
    public class Trip
    {
        public int TripId { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public string VehicleName { get; set; } = string.Empty; // not in Django response; fill manually if needed
        public string DriverName { get; set; } = string.Empty;  // not in Django response; fill manually if needed
        public double CargoWeight { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;   // "YYYY-MM-DD"
        public string? EndDate { get; set; }                    // null until completed
        public double StartOdometer { get; set; }
        public double? EndOdometer { get; set; }
        public string Status { get; set; } = "Dispatched";      // Dispatched | Completed
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class TripCreateRequest
    {
        public int TripId { get; set; }        // Django requires trip_id in the body
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public double CargoWeight { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public double StartOdometer { get; set; }
    }

    // ─── MAINTENANCE ────────────────────────────────────────────────────────
    public class MaintenanceLog
    {
        public int MaintenanceId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty; // not in Django response; kept for UI use
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceDate { get; set; } = string.Empty;     // "YYYY-MM-DD"
        public string NextServiceDue { get; set; } = string.Empty;  // "YYYY-MM-DD"
        public double Cost { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    // ─── FUEL LOG ────────────────────────────────────────────────────────────
    public class FuelLog
    {
        public int FuelId { get; set; }
        public int VehicleId { get; set; }
        public int? TripId { get; set; }
        public string VehicleName { get; set; } = string.Empty; // not in Django response; kept for UI use
        public double Liters { get; set; }
        public double Cost { get; set; }
        public string FuelDate { get; set; } = string.Empty;    // "YYYY-MM-DD"
    }

    // ─── EXPENSE ─────────────────────────────────────────────────────────────
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty; // not in Django response; kept for UI use
        public string ExpenseType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string ExpenseDate { get; set; } = string.Empty; // "YYYY-MM-DD"
        public string Remarks { get; set; } = string.Empty;
    }

    // ─── REVENUE ─────────────────────────────────────────────────────────────
    public class Revenue
    {
        public int RevenueId { get; set; }
        public int TripId { get; set; }
        public double RevenueAmount { get; set; }
        public string ReceivedDate { get; set; } = string.Empty; // "YYYY-MM-DD"
    }

    // ─── DASHBOARD ───────────────────────────────────────────────────────────
    public class DashboardStats
    {
        public int ActiveFleet { get; set; }
        public int MaintenanceAlerts { get; set; }
        public double UtilizationRate { get; set; }
        public int PendingCargo { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalExpenses { get; set; }
    }

    // ─── API RESPONSE WRAPPER ────────────────────────────────────────────────
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Kept so existing UI code that uses PagedResponse<T> still compiles.
    public class PagedResponse<T>
    {
        public int Count { get; set; }
        public List<T> Results { get; set; } = new();
    }

    // ─── DJANGO LIST RESPONSE WRAPPERS ───────────────────────────────────────
    // Django wraps every list in a named key, e.g. { "vehicles": [...] }.
    // These classes let ReadFromJsonAsync deserialise them correctly.

    public class VehicleListResponse
    {
        public List<Vehicle> Vehicles { get; set; } = new();
    }

    public class DriverListResponse
    {
        public List<Driver> Drivers { get; set; } = new();
    }

    public class TripListResponse
    {
        public List<Trip> Trips { get; set; } = new();
    }

    public class MaintenanceListResponse
    {
        public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();
    }

    public class FuelLogListResponse
    {
        public List<FuelLog> FuelLogs { get; set; } = new();
    }

    public class ExpenseListResponse
    {
        public List<Expense> Expenses { get; set; } = new();
    }

    public class RevenueListResponse
    {
        public List<Revenue> Revenues { get; set; } = new();
    }

    // ─── ROLES ───────────────────────────────────────────────────────────────
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class RoleListResponse
    {
        public List<Role> Roles { get; set; } = new();
    }

    // ─── COMPUTED / ACTION RESPONSES ─────────────────────────────────────────

    /// <summary>POST /api/fuel/efficiency/ → { trip_id, distance_km, total_liters, fuel_efficiency_km_per_liter }</summary>
    public class FuelEfficiencyResponse
    {
        public int TripId { get; set; }
        public double DistanceKm { get; set; }
        public double TotalLiters { get; set; }
        public double FuelEfficiencyKmPerLiter { get; set; }
    }

    /// <summary>POST /api/expenses/total/ → { vehicle_id, total_expense }</summary>
    public class TotalExpenseResponse
    {
        public int VehicleId { get; set; }
        public double TotalExpense { get; set; }
    }

    /// <summary>POST /api/revenue/roi/ → { vehicle_id, total_revenue, total_expense, roi }</summary>
    public class RoiResponse
    {
        public int VehicleId { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalExpense { get; set; }
        public double Roi { get; set; }
    }

    /// <summary>Generic message/error response returned by create, update, delete, deactivate endpoints.</summary>
    public class MessageResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}
