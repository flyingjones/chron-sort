using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace ImageSorter.Logging;

public class StopwatchLogFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private StopwatchLogFormatterOptions _formatterOptions;

    public StopwatchLogFormatter(IOptionsMonitor<StopwatchLogFormatterOptions> options)
        // Case insensitive
        : base("stopwatchLogFormatter") =>
        (_optionsReloadToken, _formatterOptions) =
        (options.OnChange(ReloadLoggerOptions), options.CurrentValue);

    private void ReloadLoggerOptions(StopwatchLogFormatterOptions options) =>
        _formatterOptions = options;

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        string? message =
            logEntry.Formatter?.Invoke(
                logEntry.State, logEntry.Exception);

        if (message is null)
        {
            return;
        }

        var timeSpan = TimeSpan.FromMilliseconds(_formatterOptions.Stopwatch.ElapsedMilliseconds);
        textWriter.Write(timeSpan.ToString("c"));
        textWriter.Write(" ");
        textWriter.WriteLine($"[{logEntry.LogLevel:G}]");

        const string tab = "    ";
        var newLineReplacement = $"{Environment.NewLine}{tab}";
        textWriter.Write(tab);
        textWriter.WriteLine(message.Replace(Environment.NewLine, newLineReplacement));
        if (logEntry.Exception != null)
        {
            textWriter.Write(tab);
            textWriter.WriteLine(logEntry.Exception.ToString().Replace(Environment.NewLine, newLineReplacement));
        }
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}