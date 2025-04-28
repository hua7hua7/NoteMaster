using Serilog;
using Serilog.Events;

namespace NoteMasterV0._1.Helpers
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "NoteMaster",
            "logs",
            "log.txt");

        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void LogInformation(string message)
        {
            Log.Information(message);
        }

        public static void LogError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                Log.Error(exception, message);
            }
            else
            {
                Log.Error(message);
            }
        }

        public static void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public static void LogDebug(string message)
        {
            Log.Debug(message);
        }
    }
} 