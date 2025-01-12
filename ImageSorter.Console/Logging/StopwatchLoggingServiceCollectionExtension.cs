using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Logging;

public static class StopwatchLoggingServiceCollectionExtension
{
    [UnconditionalSuppressMessage("Aot", "IL3050:RequiresDynamicCode", Justification = "Logger works as expected")]
    [UnconditionalSuppressMessage("Aot", "IL2026:RequiresUnreferencedCode", Justification = "Logger works as expected")]
    public static IServiceCollection AddStopwatchLogger(this IServiceCollection serviceCollection, LogLevel logLevel)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        // works perfectly with native aot compilation, so warning can be ignored
        serviceCollection.AddLogging(builder => builder
            .AddConsole(opt => opt.FormatterName = "stopwatchLogFormatter")
            .AddConsoleFormatter<StopwatchLogFormatter, StopwatchLogFormatterOptions>(opt => opt.Stopwatch = stopWatch)
            .AddFilter("ImageSorter", logLevel));
        return serviceCollection;
    }
}