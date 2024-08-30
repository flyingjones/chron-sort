using SixLabors.ImageSharp;
using Image = System.Drawing.Image;

namespace ImageSorter.Services.DateParser.MetaData;

public class FileMetaDataHandle : IDisposable
{
    public required string FilePath { get; set; }

    public bool LoadFailed => ImageInfo == null && Image == null;

    /// <summary>
    /// Is only present if the OsAgnosticMetaDataParser is used
    /// </summary>
    public ImageInfo? ImageInfo { get; set; }

    /// <summary>
    /// Is only present if the WindowsMetaDataDateParser is used
    /// </summary>
    public Image? Image { get; set; }
    
    public FileStream? FileStream { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Image != null)
            {
#pragma warning disable CA1416
                Image.Dispose();
#pragma warning restore CA1416
            }

            FileStream?.Dispose();
        }
    }
}