using Serilog;
using Serilog.Core;

namespace AdmAssist.Services
{
    public static class Loggers
    {
        private static Logger _hostOperationsLogger;

        public static Logger HostOperationsLogger
        {
            get
            {
                if (_hostOperationsLogger == null)
                    _hostOperationsLogger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.RollingFile("logs\\host-operations-{Date}.txt", fileSizeLimitBytes: 104857600 /* 100 MB*/)
                        .CreateLogger();
                return _hostOperationsLogger;
            }
        }
    }
}
