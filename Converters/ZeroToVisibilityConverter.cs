using Avalonia.Data.Converters;
using System;

namespace ORControlPanelNew.Converters
{
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int count && count == 0)
                return true; // IsVisible = true
            return false;    // IsVisible = false
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 