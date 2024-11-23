namespace ImageSorter.Services.ProgressLogger;

public interface IProgressLogger<T>
{
    void LogStart(string? message, params object?[] args);
    
    void LogProgress(double progress);

    void LogProgressFinished();
}