using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ORControlPanelNew.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string input || parameter is not string param)
            {
              
                return AvaloniaProperty.UnsetValue;
            }

            //Debug.WriteLine($"StringToBrushConverter: Converting value={input}, parameter={param}");

            var map = new Dictionary<string, string>();
            foreach (var pair in param.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split(':');
                if (kv.Length == 2)
                {
                    map[kv[0]] = kv[1];
                    //Debug.WriteLine($"StringToBrushConverter: Mapping {kv[0]} to {kv[1]}");
                }
            }

            if (!map.TryGetValue(input, out var resourceKey))
            {
                //Debug.WriteLine($"StringToBrushConverter: No mapping found for input={input} in parameter={param}");
                return AvaloniaProperty.UnsetValue;
            }

            if (Application.Current?.TryFindResource(resourceKey, out var found) == true)
            {
                //Debug.WriteLine($"StringToBrushConverter: Found resource for key={resourceKey}, value={found}");
                return found;
            }

            //Debug.WriteLine($"StringToBrushConverter: Resource not found for key={resourceKey}");
            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}