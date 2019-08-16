using System;

namespace RemoteQueries.Types
{
    public class QueryParameter
    {
        public string Name { get; internal set; }
        public Type Type { get; internal set; }
        public RemoteQueryGroups Group { get; internal set; }

        public QueryParameter(string name, Type type, RemoteQueryGroups group)
        {
            Name = name;
            Type = type;
            Group = group;
        }
    }
}
