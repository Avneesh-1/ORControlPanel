using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ORControlPanelNew.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isOn && parameter is string colorParams)
            {
                var colors = colorParams.Split(',');
                if (colors.Length == 2)
                {
                    var color = isOn ? colors[0].Trim() : colors[1].Trim();
                    return SolidColorBrush.Parse(color);
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 