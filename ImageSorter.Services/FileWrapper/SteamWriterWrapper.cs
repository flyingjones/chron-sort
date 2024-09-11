namespace ImageSorter.Services.FileWrapper;

/// <inheritdoc cref="IStreamWriterWrapper"/>
public sealed class SteamWriterWrapper : IStreamWriterWrapper, IAsyncDisposable
{
    private readonly StreamWriter _streamWriter;

    public SteamWriterWrapper(StreamWriter streamWriter)
    {
        _streamWriter = streamWriter;
    }

    /// <inheritdoc cref="StreamWriter.Dispose()"/>
    public void Dispose()
    {
        _streamWriter.Dispose();
    }

    /// <inheritdoc cref="StreamWriter.DisposeAsync()"/>
    public async ValueTask DisposeAsync()
    {
        await _streamWriter.DisposeAsync();
    }

    /// <inheritdoc cref="IStreamWriterWrapper.WriteLine"/>
    public void WriteLine(string? value)
    {
        _streamWriter.WriteLine(value);
    }

    /// <inheritdoc cref="IStreamWriterWrapper.Write"/>
    public void Write(string? value)
    {
        _streamWriter.Write(value);
    }
}