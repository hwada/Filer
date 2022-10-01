using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Filer.Converter
{
    class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v && v)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                return v == Visibility.Visible;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
