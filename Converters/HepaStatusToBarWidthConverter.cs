using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ORControlPanelNew.Converters
{
    public class HepaStatusToBarWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status == "GOOD")
                    return 170;
                if (status == "BAD")
                    return 30;
            }
            return 30; // Default to small width
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
} 