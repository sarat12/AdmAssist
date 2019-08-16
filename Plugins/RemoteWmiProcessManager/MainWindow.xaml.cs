namespace RemoteWmiProcessManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(MainViewModel mvm)
        {
            InitializeComponent();
            DataContext = mvm;
        }
    }
}
