using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AdmAssist.Models;

namespace AdmAssist.Views.Converters
{
    public class IsHostCommandNodeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType() == typeof(HostCommandNode) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
