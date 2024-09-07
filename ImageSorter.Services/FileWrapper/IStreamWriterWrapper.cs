namespace ImageSorter.Services.FileWrapper;

/// <summary>
/// Wrapper for the <see cref="StreamWriter"/> class to enable unit testing
/// </summary>
public interface IStreamWriterWrapper : IDisposable, IAsyncDisposable
{
    void WriteLine(string? value);

    void Write(string? value);
}