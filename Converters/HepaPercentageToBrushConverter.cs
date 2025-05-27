using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ORControlPanelNew.Converters
{
    public class HepaPercentageToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                if (percent > 50)
                    return new SolidColorBrush(Colors.Green);
                if (percent >= 30)
                    return new SolidColorBrush(Colors.Yellow);
                return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
} 