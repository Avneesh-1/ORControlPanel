using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ORControlPanelNew.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isOn && parameter is string textParams)
            {
                var texts = textParams.Split(',');
                if (texts.Length == 2)
                {
                    return isOn ? texts[0].Trim() : texts[1].Trim();
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