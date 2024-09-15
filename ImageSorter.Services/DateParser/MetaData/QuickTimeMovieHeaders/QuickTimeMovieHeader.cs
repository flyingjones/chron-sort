namespace ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;

public enum QuickTimeMovieHeader
{
    // https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom/creation_time
    CreationTime = 3,
    
    // https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom/modification_time
    ModificationTime = 4
}