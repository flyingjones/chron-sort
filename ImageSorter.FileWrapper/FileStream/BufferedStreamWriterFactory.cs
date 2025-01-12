namespace ImageSorter.FileHandling.FileStream;

/// <inheritdoc cref="IBufferedStreamWriterFactory"/>
public class BufferedStreamWriterFactory : IBufferedStreamWriterFactory
{
    /// <inheritdoc cref="IBufferedStreamWriterFactory.CreateStreamWriter"/>
    public IStreamWriterWrapper CreateStreamWriter(string filePath, FileMode fileMode)
    {
        // disposing the StreamWriterWrapper will chain dispose everything
        var fileStream = new System.IO.FileStream(filePath, fileMode);
        var bufferedStream = new BufferedStream(fileStream);
        return new SteamWriterWrapper(new StreamWriter(bufferedStream));
    }
}