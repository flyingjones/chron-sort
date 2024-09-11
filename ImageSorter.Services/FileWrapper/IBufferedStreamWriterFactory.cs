namespace ImageSorter.Services.FileWrapper;

/// <summary>
/// Factory for creating <see cref="IStreamWriterWrapper"/> instances
/// </summary>
public interface IBufferedStreamWriterFactory
{
    /// <summary>
    /// Creates a new <see cref="IStreamWriterWrapper"/> which uses a <see cref="StreamWriter"/> with an underlying
    /// <see cref="BufferedStream"/>. The <see cref="FileStream"/> is opened using the provided <paramref name="fileMode"/>
    /// </summary>
    /// <param name="filePath">path passed to the underlying <see cref="FileStream"/></param>
    /// <param name="fileMode">file mode passed to the underlying <see cref="FileStream"/></param>
    /// <returns></returns>
    IStreamWriterWrapper CreateStreamWriter(string filePath, FileMode fileMode);
}