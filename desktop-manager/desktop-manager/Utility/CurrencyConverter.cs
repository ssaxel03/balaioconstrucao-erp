using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace desktop_manager.Utility;

public class CurrencyConverter : IValueConverter
{
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return $"{decimalValue} €"; // Display mode: Show €
        }
        return value;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            stringValue = stringValue.Replace("€", "").Trim(); // Remove €
            if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result; // Editing mode: Return only the number
            }
        }
        return 0m; // Default fallback
    }
    
}