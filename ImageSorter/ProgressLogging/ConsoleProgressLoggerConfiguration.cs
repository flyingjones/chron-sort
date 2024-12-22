namespace ImageSorter.ProgressLogging;

public class ConsoleProgressLoggerConfiguration
{
    public int ProgressBarWidth => 80;

    public double RefreshThreshold => 0.001d;

    /// <summary>
    /// Char showing that a part of the progress bar is fully finished
    /// </summary>
    public char FullBarChar => ProgressCharsString[^1];

    /// <summary>
    /// Char showing that a part of the progress bar is not started
    /// </summary>
    public char EmptyBarChar => ProgressCharsString[0];

    /// <summary>
    /// Progress characters from left to right (0% progress to 100%)
    /// </summary>
    /// <remarks>
    /// Should contain at least 2 characters.
    /// </remarks>
    public string ProgressCharsString { get; set; } = ".#";
}