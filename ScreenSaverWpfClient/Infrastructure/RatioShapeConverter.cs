using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScreenSaverWpfClient.Infrastructure
{
    [ValueConversion(typeof(object), typeof(double))]
    internal class RatioShapeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (double)value + (double)parameter;

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
    }
}
