using RemoteQueries.Types;

namespace AdmAssist.Models
{
    public class RemoteQueryParameter
    {
        public string Name { get; set; }
        public RemoteQueryGroups Group { get; set; }
        public bool Enabled { get; set; }

        public RemoteQueryParameter(string name, RemoteQueryGroups group)
        {
            Name = name;
            Group = group;
        }
    }
}
