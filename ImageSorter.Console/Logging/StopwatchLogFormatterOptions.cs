using System.Diagnostics;
using Microsoft.Extensions.Logging.Console;

namespace ImageSorter.Logging;

public class StopwatchLogFormatterOptions : ConsoleFormatterOptions
{
    public required Stopwatch Stopwatch { get; set; }

    public StopwatchLogFormatterOptions()
    {
        IncludeScopes = true;
    }
}