using System;
using System.Collections;
using AdmAssist.Models;

namespace AdmAssist.Helpers
{
    class StringComparer : IComparer
    {
        private readonly bool _order;
        private readonly string _columnName;

        public StringComparer(bool order, string columnName)
        {
            _order = order;
            _columnName = columnName;
        }

        private int Compare(string x, string y)
        {
            return _order ? string.Compare(x, y, StringComparison.Ordinal) : string.Compare(y, x, StringComparison.Ordinal);
        }

        public int Compare(object x, object y)
        {
            return Compare((string)((NotifyDynamicDictionary)x)[_columnName], (string)((NotifyDynamicDictionary)y)[_columnName]);
        }
    }
}
