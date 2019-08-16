using AdmAssist.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AdmAssist.Views.Converters
{
    public class AppStateNotScanningToTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AppState)value != AppState.Scaning;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
