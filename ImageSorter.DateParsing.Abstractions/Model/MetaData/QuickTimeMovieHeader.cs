namespace ImageSorter.DateParsing.Abstractions.Model.MetaData;

public enum QuickTimeMovieHeader
{
    // https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom/creation_time
    CreationTime = 3,
    
    // https://developer.apple.com/documentation/quicktime-file-format/movie_header_atom/modification_time
    ModificationTime = 4
}