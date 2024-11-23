using ImageSorter.Services.ProgressLogger;

namespace ImageSorter.ProgressLogging;

public class NoOperationProgressLogger<T> : IProgressLogger<T>
{
    public void LogStart(string? message, params object?[] args)
    {
        // no operation
    }

    public void LogProgress(double progress)
    {
        // no operation
    }

    public void LogProgressFinished()
    {
        // no operation
    }
}