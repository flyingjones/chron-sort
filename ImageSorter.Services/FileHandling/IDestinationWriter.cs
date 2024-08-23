namespace ImageSorter.Services.FileHandling;

public interface IDestinationWriter
{
    void CopyFile(string sourcePath, DateTime dateTime);
}