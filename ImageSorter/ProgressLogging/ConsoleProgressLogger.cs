using System.Diagnostics;
using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.Logging;

namespace ImageSorter.ProgressLogging;

/// <summary>
/// Progress Logger which writes an animated progress bar to the last line of the console
/// </summary>
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
    /// <inheritdoc/>

    public void LogStart(string? message, params object?[] args)
    {
        _logger.LogInformation(message, args);
        Thread.Sleep(50);
        _stopwatch.Start();
    }

    /// <summary>
    /// Write the progress message to the console
    /// </summary>
    private void LogProgressInternal(double progress)
    {
        // number of times the finished char should be rendered
        var fullCharsCount = (int)Math.Floor(progress * _configuration.ProgressBarWidth);
        // number of times the not started char should be rendered
        var spaceCount = _configuration.ProgressBarWidth - (int)Math.Ceiling(progress * _configuration.ProgressBarWidth);
        // progress of the current character
        var diff = 1 - (Math.Ceiling(progress * _configuration.ProgressBarWidth) - progress * _configuration.ProgressBarWidth);
        char? currentChar = null;
        if ((int)Math.Floor(progress * _configuration.ProgressBarWidth) != (int)Math.Ceiling(progress * _configuration.ProgressBarWidth))
        {
            // get the current character from the character array
            currentChar = _configuration.ProgressCharsString[(int)Math.Floor(diff * _configuration.ProgressCharsString.Length)];
        }

        var bars = new string(_configuration.FullBarChar, fullCharsCount);
        var spaces = new string(_configuration.EmptyBarChar, spaceCount);
        var percentage = progress * 100d;
        var estimatedTimeLeft = GetEstimatedTimeLeft(progress);

        Console.Write(currentChar != null
            ? $"\rProgress: [{percentage,5:##0.0} %] [{bars}{currentChar}{spaces}] [{estimatedTimeLeft} left]   "
            : $"\rProgress: [{progress,5:##0.0} %] [{bars}{spaces}] [{estimatedTimeLeft} left]   ");

        _lastPercentage = progress;
    }

    /// <summary>
    /// Estimate the remaining time based on the time passed for the current progress.
    /// </summary>
    /// <remarks>
    /// This could be improved by using a moving average
    /// </remarks>
    private string GetEstimatedTimeLeft(double progress)
    {
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

        return estimatedTimeLeft;
    }

    /// <inheritdoc/>
    public void LogProgress(double progress)
    {
        // only log progress if sufficient progress has been made
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

    /// <inheritdoc/>
    public void LogProgressFinished()
    {
        var emptyCharsCount = _configuration.ProgressBarWidth + 37;
        Console.WriteLine("\r" + new string(' ', emptyCharsCount) + "\r");
        _lastPercentage = 1d;
        _stopwatch.Stop();

        var timeTaken = TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds);
        _logger.LogInformation(@"Finished in {timeTaken}", timeTaken);
        
        _stopwatch.Reset();
    }
}