using AdmAssist.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AdmAssist.Views.Converters
{
    class FilterColumnsToHighlightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
            var columnName = values[1] as string;

            if (mvm == null || columnName == null)
                return string.Empty;

            var targetSelectebleItem = mvm.FilterColumns.FirstOrDefault(i => i.ObjectData == columnName);

            return targetSelectebleItem != null && targetSelectebleItem.IsSelected ? mvm.AcknowledgedFilterString : string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
