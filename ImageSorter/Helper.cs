namespace ImageSorter;

public static class Helper
{
    public static string FileEnding(this string filePath)
    {
        return filePath.Split(".")[^1];
    }
}