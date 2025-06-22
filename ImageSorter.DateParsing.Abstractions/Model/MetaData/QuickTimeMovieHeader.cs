namespace ImageSorter.DateParsing.Abstractions.Model.MetaData;

/// <summary>
/// Enum for identifying tags in a <see href="https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom">Quick Time Movie header</see>
/// </summary>
/// <remarks>
/// Values represent the tag type and are passed to a library to load the tag values so don't change them!
/// </remarks>
public enum QuickTimeMovieHeader
{
    /// <summary>
    /// <see href="https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom/creation_time">Apple Documentation</see>
    /// </summary>
    CreationTime = 3,
    
    // 
    /// <summary>
    /// <see href="https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom/modification_time">Apple Documentation</see>
    /// </summary>
    ModificationTime = 4
}