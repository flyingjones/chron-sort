namespace ImageSorter.Services.FileWrapper;

/// <summary>
/// Wrapper for the <see cref="StreamWriter"/> class to enable unit testing
/// </summary>
public interface IStreamWriterWrapper : IDisposable, IAsyncDisposable
{
    /// <inheritdoc cref="StreamWriter.WriteLine(string)"/>
    void WriteLine(string? value);

    /// <inheritdoc cref="StreamWriter.Write(string)"/>
    void Write(string? value);
}