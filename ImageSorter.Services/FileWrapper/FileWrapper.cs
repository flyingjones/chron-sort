namespace ImageSorter.Services.FileWrapper;

public class FileWrapper : IFileWrapper
{
    public bool Exists(string? path)
    {
        return File.Exists(path);
    }

    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        File.Move(sourceFileName, destFileName, overwrite);
    }
}