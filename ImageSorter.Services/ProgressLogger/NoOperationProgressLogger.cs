namespace ImageSorter.Services.ProgressLogger;

/// <summary>
/// ProgressLogger which doesn't log at all. Useful when output is piped to a file.
/// </summary>
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