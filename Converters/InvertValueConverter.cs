using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ORControlPanelNew.Converters
{
    public class InvertValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return 100 - doubleValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return 100 - doubleValue;
            }
            return value;
        }
    }
} 