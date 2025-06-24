using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ORControlPanelNew.Converters
{
    public class HepaPercentageToBarWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                double width = Math.Max(0, Math.Min(1, percent / 100.0)) * 170;
                return width;
            }
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
} 