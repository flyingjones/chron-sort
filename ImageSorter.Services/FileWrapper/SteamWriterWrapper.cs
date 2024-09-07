namespace ImageSorter.Services.FileWrapper;

public class SteamWriterWrapper : IStreamWriterWrapper, IAsyncDisposable
{
    private readonly StreamWriter _streamWriter;

    public SteamWriterWrapper(StreamWriter streamWriter)
    {
        _streamWriter = streamWriter;
    }

    public void Dispose()
    {
        _streamWriter.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _streamWriter.DisposeAsync();
    }

    public void WriteLine(string? value)
    {
        _streamWriter.WriteLine(value);
    }

    public void Write(string? value)
    {
        _streamWriter.Write(value);
    }
}