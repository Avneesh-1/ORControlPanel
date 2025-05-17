using Avalonia.Data.Converters;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using ORControlPanelNew.Models.GasMonitoring;
using System.Linq;

namespace ORControlPanelNew.Converters
{
    public class GasAlertToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<GasStatus> gases && parameter is string gasName)
            {
                var gas = gases.FirstOrDefault(g => g.Name == gasName);
                return gas?.IsAlert ?? false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}