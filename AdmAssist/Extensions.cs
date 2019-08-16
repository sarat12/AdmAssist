using System.Collections.Generic;

namespace AdmAssist
{
    public static class Extensions
    {
        public static void AddRange(this Dictionary<string, object> self, Dictionary<string, object> source)
        {
            foreach (var o in source)
            {
                self.Add(o.Key, o.Value);
            }
        }
    }
}
