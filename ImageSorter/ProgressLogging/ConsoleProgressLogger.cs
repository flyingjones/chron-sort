using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.Logging;

namespace ImageSorter.ProgressLogging;

public class ConsoleProgressLogger<T> : IProgressLogger<T>
{
    private readonly int _progressBarLength;
    private readonly double _refreshInterval;
    private readonly ILogger<T> _logger;
    private double _lastPercentage = -1d;

    public ConsoleProgressLogger(ConsoleProgressLoggerConfiguration configuration, ILogger<T> logger)
    {
        _progressBarLength = configuration.ProgressBarWidth;
        _refreshInterval = configuration.RefreshThreshold;
        _logger = logger;
    }

    public void LogStart(string? message, params object?[] args)
    {
        _logger.LogInformation(message, args);
    }

    private void LogProgressInternal(double progress)
    {
        var fullCharsCount = (int)Math.Floor(progress * _progressBarLength);
        var spaceCount = _progressBarLength - (int)Math.Ceiling(progress * _progressBarLength);
        var diff = 1 - (Math.Ceiling(progress * _progressBarLength) - progress * _progressBarLength);
        var currentChar = string.Empty;
        if ((int)Math.Floor(progress * _progressBarLength) != (int)Math.Ceiling(progress * _progressBarLength))
        {
            currentChar = ConsoleProgressLoggerHelpers.ProgressChars[(int)Math.Floor(diff * 8)];
        }

        var bars = new string('\u2588', fullCharsCount);
        var spaces = new string(' ', spaceCount);
        Console.Write($"\rProgress: [{bars}{currentChar}{spaces}] {progress:P}");
    }

    public void LogProgress(double progress)
    {
        if (progress < 0d)
        {
            LogProgressInternal(0d);
        }
        else if (progress > 1d)
        {
            LogProgressInternal(1d);
        }
        else
        {
            LogProgressInternal(progress);
        }
    }

    public void LogProgressFinished()
    {
        var emptyCharsCount = _progressBarLength + 20;
        Console.WriteLine("\r" + new string(' ', emptyCharsCount) + "\r");
        _lastPercentage = 1d;
    }
}