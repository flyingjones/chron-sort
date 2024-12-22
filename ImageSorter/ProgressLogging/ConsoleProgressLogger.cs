using System.Diagnostics;
using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.Logging;

namespace ImageSorter.ProgressLogging;

public class ConsoleProgressLogger<T> : IProgressLogger<T>
{
    private readonly ConsoleProgressLoggerConfiguration _configuration;
    private readonly ILogger<T> _logger;
    private double _lastPercentage = -1d;
    private readonly Stopwatch _stopwatch;

    public ConsoleProgressLogger(
        ConsoleProgressLoggerConfiguration configuration,
        ILogger<T> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    public void LogStart(string? message, params object?[] args)
    {
        _stopwatch.Start();
        _logger.LogInformation(message, args);
    }

    private void LogProgressInternal(double progress)
    {
        var fullCharsCount = (int)Math.Floor(progress * _configuration.ProgressBarWidth);
        var spaceCount = _configuration.ProgressBarWidth - (int)Math.Ceiling(progress * _configuration.ProgressBarWidth);
        var diff = 1 - (Math.Ceiling(progress * _configuration.ProgressBarWidth) - progress * _configuration.ProgressBarWidth);
        char? currentChar = null;
        if ((int)Math.Floor(progress * _configuration.ProgressBarWidth) != (int)Math.Ceiling(progress * _configuration.ProgressBarWidth))
        {
            currentChar = _configuration.ProgressCharsString[(int)Math.Floor(diff * _configuration.ProgressCharsString.Length)];
        }

        var bars = new string(_configuration.FullBarChar, fullCharsCount);
        var spaces = new string(_configuration.EmptyBarChar, spaceCount);

        var percentage = progress * 100d;
        var missingProgress = 1d - progress;

        var timePassed = _stopwatch.ElapsedMilliseconds;
        var estimatedTimeLeft = "?";
        if (progress > 0d)
        {
            var estimatedLeftMillis = (long)(timePassed / progress * missingProgress);
            var estimatedTimeSpan = TimeSpan.FromMilliseconds(estimatedLeftMillis);
            if (estimatedTimeSpan > TimeSpan.FromHours(1))
            {
                estimatedTimeLeft = $@"{estimatedTimeSpan:h\ \h}";
            }
            else if (estimatedTimeSpan > TimeSpan.FromMinutes(1))
            {
                estimatedTimeLeft =  $@"{estimatedTimeSpan:m\ \m\i\n}";
            }
            else
            {
                estimatedTimeLeft = $@"{estimatedTimeSpan:s\ \s}";
            }
        }

        Console.Write(currentChar != null
            ? $"\rProgress: [{percentage,5:##0.0} %] [{bars}{currentChar}{spaces}] [{estimatedTimeLeft} left]   "
            : $"\rProgress: [{progress,5:##0.0} %] [{bars}{spaces}] [{estimatedTimeLeft} left]   ");

        _lastPercentage = progress;
    }

    public void LogProgress(double progress)
    {
        if (Math.Abs(progress - _lastPercentage) < _configuration.RefreshThreshold)
        {
            return;
        }
        
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
        var emptyCharsCount = _configuration.ProgressBarWidth + 37;
        Console.WriteLine("\r" + new string(' ', emptyCharsCount) + "\r");
        _lastPercentage = 1d;
        _stopwatch.Stop();
        _stopwatch.Reset();
    }
}