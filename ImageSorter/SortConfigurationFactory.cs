using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

public static class SortConfigurationFactory
{
    public static IServiceCollection ConfigureSorting(
        this IServiceCollection serviceCollection,
        string[] sortConfig)
    {
        var priority = 0;

        var logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILogger<IDateParserImplementation>>();

        foreach (var configEntry in sortConfig)
        {
            var splat = configEntry.Split(':', 2);
            if (splat.Length != 2 || !Enum.TryParse(splat[0], out SortType sortType))
            {
                throw new ArgumentException($"invalid config string {configEntry} (invalid type)", nameof(sortConfig));
            }

            if (sortType == SortType.ExifTag)
            {
                if (!Enum.TryParse(splat[1], out ExifTagId exifTagId))
                {
                    throw new ArgumentException($"invalid config string {configEntry} (invalid exif tag)",
                        nameof(sortConfig));
                }

                serviceCollection.AddMetaDataParser(exifTagId, priority, logger);
            }
            else if (sortType == SortType.FileName)
            {
                var ignoresAfter = DateTime.Now.Date.AddYears(1);
                serviceCollection.AddSingleton<IDateParserImplementation>(new FilenameDateParser(splat[1], priority,
                    ignoresAfter));
            }

            priority++;
        }

        return serviceCollection;
    }

    public static readonly string[] DefaultSorting = new[]
    {
        $"{SortType.ExifTag:G}:{ExifTagId.DateTimeOriginal:G}",
        $"{SortType.ExifTag:G}:{ExifTagId.DateTimeDigitized:G}",
        $"{SortType.ExifTag:G}:{ExifTagId.DateTime:G}",
        $"{SortType.FileName:G}:.*(?<year>20[0-9]{{2}}|19[0-9]{{2}})-(?<month>0[0-9]|1[0-9])-(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*",
        $"{SortType.FileName:G}:.*(?<year>20[0-9]{{2}}|19[0-9]{{2}})(?<month>0[0-9]|1[0-9])(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*",
    };
}