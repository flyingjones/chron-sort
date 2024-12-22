namespace ImageSorter.Services.ProgressLogger;

/// <summary>
/// Interface for a service which can report the progress of a task to the user
/// </summary>
/// <typeparam name="T">The of the Service which uses the logger</typeparam>
public interface IProgressLogger<T>
{
    /// <summary>
    /// Log that the task was started
    /// </summary>
    /// <param name="message">Message template</param>
    /// <param name="args">Message arguments</param>
    void LogStart(string? message, params object?[] args);
    
    /// <summary>
    /// Log the current progress
    /// </summary>
    /// <param name="progress">progress as a value from 0 to 1</param>
    void LogProgress(double progress);

    /// <summary>
    /// Log that the operation is completed
    /// </summary>
    void LogProgressFinished();
}