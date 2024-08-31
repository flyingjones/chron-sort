namespace ImageSorter.Services.DateParser.MetaData;

public enum ExifTagId : ushort
{
    /// <summary>
    /// Date and time of image creation.
    /// See Section 8: Baseline Fields.
    /// </summary>
    DateTime = 0x0132,
    
    /// <summary>
    /// DateTimeOriginal
    /// </summary>
    DateTimeOriginal = 0x9003,

    /// <summary>
    /// DateTimeDigitized
    /// </summary>
    DateTimeDigitized = 0x9004,
}