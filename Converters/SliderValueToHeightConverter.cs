using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ORControlPanelNew.Converters
{
    public class SliderValueToHeightConverter : IValueConverter
    {
        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;
        public double TrackHeight { get; set; } = 200;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val)
            {
                double percent = (val - Minimum) / (Maximum - Minimum);
                return percent * TrackHeight;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 