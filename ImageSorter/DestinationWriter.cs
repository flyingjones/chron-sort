using System.Collections.Concurrent;

namespace ImageSorter;

public class DestinationWriter
{
    private readonly string _path;
    private readonly IDictionary<int, IDictionary<int, bool>> _filePaths;

    public DestinationWriter(string path)
    {
        _path = path;
        _filePaths = new ConcurrentDictionary<int, IDictionary<int, bool>>();
        Directory.CreateDirectory(path);
    }

    public void CopyFile(string sourcePath, DateTime takenDate)
    {
        var year = takenDate.Year;
        var month = takenDate.Month;
        var yearPath = $"{_path}/{year:0000}";
        var monthPath = $"{yearPath}/{month:00}";
        if (!_filePaths.ContainsKey(year))
        {
            Directory.CreateDirectory(yearPath);
            _filePaths[year] = new ConcurrentDictionary<int, bool>();
        }

        if (!_filePaths[year].ContainsKey(month))
        {
            Directory.CreateDirectory(monthPath);
            _filePaths[year][month] = true;
        }

        var fileName = System.IO.Path.GetFileName(sourcePath);
        try
        {
            File.Copy(sourcePath, $"{monthPath}/{fileName}", false);
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"couldn't insert file {monthPath}/{fileName}");
            // Console.WriteLine(ex);
        }
        
    }
}