using ImageParser.Utils.RandomWrapper;
using ImageSorter.FileWrapper.Abstractions.CaseSensitivity;

namespace ImageSorter.FileHandling.CaseSensitivity;

public class CaseSensitivityAutoDetector : ICaseSensitivityAutoDetector
{
    private readonly RandomStringGenerator _randomStringGenerator = RandomStringGenerator.CreateForLowerCaseLetters();

    public bool CheckIfIsCaseSensitive(string path)
    {
        // first create file name in upper and lower
        var lowerFileName = $"fs_check_{_randomStringGenerator.GenerateRandomString(8)}.txt";
        var upperFileName = lowerFileName.ToUpper();

        var lowerCasePath = System.IO.Path.Combine(path, lowerFileName);
        var upperCasePath = System.IO.Path.Combine(path, upperFileName);

        bool upperCaseFileExists;
        
        // create the dir where the check is performed
        System.IO.Directory.CreateDirectory(path);
        
        // create file with lower casing
        using (var lowerCaseFileStream = System.IO.File.Create(lowerCasePath))
        {
            lowerCaseFileStream.Flush();

            // check if the upper case file exists
            upperCaseFileExists = System.IO.File.Exists(upperCasePath);
        }

        // clean up
        System.IO.File.Delete(lowerCasePath);

        return !upperCaseFileExists;
    }
}