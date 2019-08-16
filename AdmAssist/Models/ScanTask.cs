namespace AdmAssist.Models
{
    public class ScanTask
    {
        public NotifyDynamicDictionary Node { get; }

        public ScanTask(NotifyDynamicDictionary node)
        {
            Node = node;
        }
    }
}
