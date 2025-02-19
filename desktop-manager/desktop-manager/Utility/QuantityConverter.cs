using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace desktop_manager.Utility;

public class QuantityConverter : IValueConverter
{
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue; // Display the raw number
        }
        return 0m; // If somehow invalid, reset to 0
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result; // If valid, return number
            }
        }
        return 0m; // If invalid, reset to 0
    }
    
}