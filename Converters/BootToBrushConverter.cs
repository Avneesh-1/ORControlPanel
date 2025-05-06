using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ORControlPanelNew.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool isAlert || parameter is not string param)
            {
                Debug.WriteLine($"BoolToBrushConverter: Invalid input or parameter - value={value}, parameter={parameter}");
                return AvaloniaProperty.UnsetValue;
            }

            var map = new Dictionary<string, string>();
            foreach (var pair in param.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split(':');
                if (kv.Length == 2)
                    map[kv[0]] = kv[1];
            }

            string key = isAlert.ToString().ToLower();
            if (!map.TryGetValue(key, out var resourceKey))
            {
                Debug.WriteLine($"BoolToBrushConverter: No mapping found for isAlert={isAlert} in parameter={param}");
                return AvaloniaProperty.UnsetValue;
            }

            if (Application.Current?.TryFindResource(resourceKey, out var found) == true)
            {
                Debug.WriteLine($"BoolToBrushConverter: Found resource for key={resourceKey}, value={found}");
                return found;
            }

            Debug.WriteLine($"BoolToBrushConverter: Resource not found for key={resourceKey}");
            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}