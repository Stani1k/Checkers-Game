using Serilog;
using Serilog.Events;

namespace Checkers.Tools.Logging;

/// <summary>Configures the global Serilog logger. Logs go to %AppData%\Checkers\logs\&lt;app&gt;-yyyyMMdd.log.</summary>
public static class LoggingSetup
{
    public static string DefaultLogDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Checkers", "logs");

    public static LoggerConfiguration CreateConfiguration(string appName, string? logDirectory = null)
    {
        var path = Path.Combine(logDirectory ?? DefaultLogDirectory, $"{appName}-.log");
        return new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.WithProperty("App", appName)
            .WriteTo.File(
                path,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
    }

    public static void Initialize(string appName, Action<LoggerConfiguration>? configure = null, string? logDirectory = null)
    {
        var configuration = CreateConfiguration(appName, logDirectory);
        configure?.Invoke(configuration);
        Log.Logger = configuration.CreateLogger();
    }

    public static void Shutdown() => Log.CloseAndFlush();
}
