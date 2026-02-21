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

    public class LoginResponse
    {
        public string Access { get; set; } = string.Empty;
        public string Refresh { get; set; } = string.Empty;
        public UserProfile User { get; set; } = new();
    }
   
    public class UserProfile
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
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
        public string Status { get; set; } = "Available"; // Available | On Trip | In Shop | Retired
        public DateTime CreatedAt { get; set; }
    }

    // ─── DRIVER ─────────────────────────────────────────────────────────────
    public class Driver
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseCategory { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public double SafetyScore { get; set; }
        public string Status { get; set; } = "Off Duty"; // On Duty | Off Duty | Suspended
        public DateTime CreatedAt { get; set; }
    }

    // ─── TRIP ───────────────────────────────────────────────────────────────
    public class Trip
    {
        public int TripId { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public double CargoWeight { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double StartOdometer { get; set; }
        public double? EndOdometer { get; set; }
        public string Status { get; set; } = "Draft"; // Draft | Dispatched | Completed | Cancelled
        public DateTime CreatedAt { get; set; }
    }

    public class TripCreateRequest
    {
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
        public string VehicleName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public DateTime NextServiceDue { get; set; }
        public double Cost { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    // ─── FUEL LOG ────────────────────────────────────────────────────────────
    public class FuelLog
    {
        public int FuelId { get; set; }
        public int VehicleId { get; set; }
        public int? TripId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public double Liters { get; set; }
        public double Cost { get; set; }
        public DateTime FuelDate { get; set; }
    }

    // ─── EXPENSE ─────────────────────────────────────────────────────────────
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    // ─── REVENUE ─────────────────────────────────────────────────────────────
    public class Revenue
    {
        public int RevenueId { get; set; }
        public int TripId { get; set; }
        public double RevenueAmount { get; set; }
        public DateTime ReceivedDate { get; set; }
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

    public class PagedResponse<T>
    {
        public int Count { get; set; }
        public List<T> Results { get; set; } = new();
    }
}
