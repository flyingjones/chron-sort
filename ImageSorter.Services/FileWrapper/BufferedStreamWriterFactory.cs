namespace ImageSorter.Services.FileWrapper;

public class BufferedStreamWriterFactory : IBufferedStreamWriterFactory
{
    public IStreamWriterWrapper CreateStreamWriter(string filePath, FileMode fileMode)
    {
        // disposing the StreamWriterWrapper will chain dispose everything
        var fileStream = new FileStream(filePath, fileMode);
        var bufferedStream = new BufferedStream(fileStream);
        return new SteamWriterWrapper(new StreamWriter(bufferedStream));
    }
}