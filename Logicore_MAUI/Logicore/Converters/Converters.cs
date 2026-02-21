using System.Globalization;

namespace Logicore.Converters
{
    // ─── Maps status strings → colors ────────────────────────────────────────
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "Available" or "On Duty" or "Completed" => "#2ecc71",
                "On Trip" or "Dispatched" or "Draft"   => "#3498db",
                "In Shop" or "Off Duty"                => "#f39c12",
                "Retired" or "Cancelled" or "Suspended"=> "#e74c3c",
                _                                      => "#8899aa"
            };
        }
        public object ConvertBack(object? value, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
    }

    // ─── Busy text: "Loading...|Normal Text" ─────────────────────────────────
    public class BusyTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var parts = parameter?.ToString()?.Split('|') ?? Array.Empty<string>();
            bool isBusy = value is true;
            return isBusy ? (parts.Length > 0 ? parts[0] : "Loading...") : (parts.Length > 1 ? parts[1] : "");
        }
        public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
    }

    // ─── Returns true when string is not null/empty ───────────────────────────
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value?.ToString());
        public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
    }

    // ─── Shows element only when status matches parameter ─────────────────────
    public class StatusVisibleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value?.ToString() == parameter?.ToString();
        public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
    }

    // ─── Inverts a bool ───────────────────────────────────────────────────────
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;
        public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
    }
}
