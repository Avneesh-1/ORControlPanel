using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using ORControlPanelNew.Models.GasMonitoring;

namespace ORControlPanelNew.Converters
{
    public class GasStatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is GasLevel level)
            {
                return level switch
                {
                    GasLevel.Low => new SolidColorBrush(Colors.Yellow),
                    GasLevel.Normal => new SolidColorBrush(Colors.LimeGreen),
                    GasLevel.High => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 