namespace ImageSorter.DateParsing.Abstractions.Model.MetaData;

/// <summary>
/// Enum representing <see href="https://exiftool.org/TagNames/EXIF.html">Exif Tag IDs</see>
/// </summary>
public enum ExifTagId : ushort
{
    /// <summary>
    /// Date and time of image creation.
    /// See Section 8: Baseline Fields.
    /// </summary>
    /// <remarks>
    /// Sometimes called ModifyDate
    /// </remarks>
    DateTime = 0x0132,
    
    /// <summary>
    /// DateTimeOriginal
    /// </summary>
    /// <remarks>
    /// date/time when original image was taken
    /// </remarks>
    DateTimeOriginal = 0x9003,

    /// <summary>
    /// DateTimeDigitized
    /// </summary>
    /// <remarks>
    /// Sometimes called CreateDate
    /// </remarks>
    DateTimeDigitized = 0x9004,
}