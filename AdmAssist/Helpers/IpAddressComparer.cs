using System.Collections;
using System.Net;
using AdmAssist.Models;

namespace AdmAssist.Helpers
{
    public class IpAddressComparer : IComparer
    {
        private readonly bool _order;
        private readonly string _columnName;

        public IpAddressComparer(bool order, string columnName)
        {
            _order = order;
            _columnName = columnName;
        }

        private int Compare(IPAddress x, IPAddress y)
        {
            if (_order)
                return x.ToInt() - y.ToInt();

            return y.ToInt() - x.ToInt();
        }

        public int Compare(object x, object y)
        {
            return Compare((IPAddress)((NotifyDynamicDictionary)x)[_columnName], (IPAddress)((NotifyDynamicDictionary)y)[_columnName]);
        }
    }
}
