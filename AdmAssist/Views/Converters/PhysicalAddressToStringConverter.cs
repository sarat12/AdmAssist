using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using AdmAssist.Helpers;

namespace AdmAssist.Views.Converters
{
    class PhysicalAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PhysicalAddress)) return string.Empty;

            return ((PhysicalAddress) value).ToString(":");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
