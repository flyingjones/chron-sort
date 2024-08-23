using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Logging;

public static class StopwatchLoggingServiceCollectionExtension
{
    public static IServiceCollection AddStopwatchLogger(this IServiceCollection serviceCollection)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        serviceCollection.AddLogging(builder => builder
            .AddConsole(opt => opt.FormatterName = "stopwatchLogFormatter")
            .AddConsoleFormatter<StopwatchLogFormatter, StopwatchLogFormatterOptions>(opt => opt.Stopwatch = stopWatch));
        return serviceCollection;
    }
}