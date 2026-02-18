using Serilog;
using Serilog.Events;

namespace LocallyGDriveApi.Shared.Utils;

public static class LogUtil
{
    
    public static string SimplifyLogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => "VERBOSE",
            LogEventLevel.Debug => "DEBUG",
            LogEventLevel.Information => "INFO",
            LogEventLevel.Warning => "WARNING",
            LogEventLevel.Error => "ERROR",
            LogEventLevel.Fatal => "FATAL",
            _ => "UNKNOWN"
        };
    }
}