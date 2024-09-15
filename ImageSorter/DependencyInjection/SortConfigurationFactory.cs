using System.Collections.ObjectModel;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.DateParser.MetaData.ExifTags;
using ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter.DependencyInjection;

public static class SortConfigurationFactory
{
    public static IServiceCollection ConfigureSorting(
        this IServiceCollection serviceCollection,
        string[] sortConfig)
    {
        var priority = 0;

        // var logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILogger<IDateParserImplementation>>();

        serviceCollection.AddSingleton<IDateParserImplementation>(
            new QuickTimeMovieHeaderParser(QuickTimeMovieHeader.CreationTime, -1));

        foreach (var configEntry in sortConfig)
        {
            var splat = configEntry.Split(':', 2);
            if (splat.Length != 2 || !Enum.TryParse(splat[0], out SortType sortType))
            {
                throw new ArgumentException($"invalid config string {configEntry} (invalid type)", nameof(sortConfig));
            }

            switch (sortType)
            {
                case SortType.ExifTag:
                {
                    if (!Enum.TryParse(splat[1], out ExifTagId exifTagId))
                    {
                        throw new ArgumentException($"invalid config string {configEntry} (invalid exif tag)",
                            nameof(sortConfig));
                    }

                    serviceCollection.AddSingleton<IDateParserImplementation>(
                        new ExifTagParser(exifTagId, priority));
                    break;
                }
                case SortType.QuickTimeMovieHeader:
                {
                    if (!Enum.TryParse(splat[1], out QuickTimeMovieHeader quickTimeMovieHeader))
                    {
                        throw new ArgumentException(
                            $"invalid config string {configEntry} (invalid quickTimeMovieHeader)",
                            nameof(sortConfig));
                    }

                    serviceCollection.AddSingleton<IDateParserImplementation>(
                        new QuickTimeMovieHeaderParser(quickTimeMovieHeader, priority));
                    break;
                }
                case SortType.FileName:
                    serviceCollection.AddSingleton<IDateParserImplementation>(
                        new FilenameDateParser(splat[1], priority));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortConfig), $"Invalid {nameof(SortType)}");
            }

            priority++;
        }

        return serviceCollection;
    }

    public static readonly IReadOnlyCollection<string> DefaultSorting =
        new ReadOnlyCollection<string>(new[]
        {
            $"{SortType.ExifTag:G}:{ExifTagId.DateTimeOriginal:G}",
            $"{SortType.ExifTag:G}:{ExifTagId.DateTimeDigitized:G}",
            $"{SortType.ExifTag:G}:{ExifTagId.DateTime:G}",
            $"{SortType.QuickTimeMovieHeader:G}:{QuickTimeMovieHeader.CreationTime:G}",
            $"{SortType.QuickTimeMovieHeader:G}:{QuickTimeMovieHeader.ModificationTime:G}",
            $"{SortType.FileName:G}:.*(?<year>20[0-9]{{2}}|19[0-9]{{2}})-(?<month>0[0-9]|1[0-9])-(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*",
            $"{SortType.FileName:G}:.*(?<year>20[0-9]{{2}}|19[0-9]{{2}})(?<month>0[0-9]|1[0-9])(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*",
        });
}