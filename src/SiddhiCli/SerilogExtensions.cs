using System;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace SiddhiCli
{
    public static class SerilogExtensions
    {
        public static void ConfigureLogger(string logFileName, bool predefinedFilter = true)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile(
                    logFileName,
                    LogEventLevel.Verbose,
                    "{NewLine}{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: 50 * 1024 * 1024, // 50 mB
                    retainedFileCountLimit: 10,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5));
            if (predefinedFilter)
                logger
                    .MinimumLevel.Override("System", LogEventLevel.Error) // removing flooding events                
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error); // removing flooding events
            Log.Logger = logger.CreateLogger();
        }
    }
}