namespace ImageSorter.Services.FileWrapper;

public interface IBufferedStreamWriterFactory
{
    IStreamWriterWrapper CreateStreamWriter(string filePath, FileMode fileMode);
}