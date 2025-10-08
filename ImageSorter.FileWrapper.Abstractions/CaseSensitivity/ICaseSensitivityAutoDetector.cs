namespace ImageSorter.FileWrapper.Abstractions.CaseSensitivity;

public interface ICaseSensitivityAutoDetector
{
    /// <summary>
    /// Checks if the file system at <paramref name="path"/> is case-sensitive or not
    /// </summary>
    /// <remarks>
    /// Is done by creating a file with a random name in lower case letters and checking if the file exists by checking
    /// the upper case name
    /// </remarks>
    /// <param name="path"></param>
    /// <returns></returns>
    bool CheckIfIsCaseSensitive(string path);
}