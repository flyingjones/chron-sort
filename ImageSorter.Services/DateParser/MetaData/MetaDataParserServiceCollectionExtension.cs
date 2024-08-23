using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace ImageSorter.Services.DateParser.MetaData;

public static class MetaDataParserServiceCollectionExtension
{
    public static IServiceCollection AddMetaDataParsing(this IServiceCollection serviceCollection)
    {
        var options = new MetaDataDateParserOptions
        {
            TagOptions = new[]
            {
                // see https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.propertyitem.id
                new MetaDataImageTagOption
                {
                    // PropertyTagExifDTOrig
                    Tag = ExifTag.DateTimeOriginal
                },
                new MetaDataImageTagOption
                {
                    // PropertyTagExifDTDigitized
                    Tag = ExifTag.DateTimeDigitized
                },
                new MetaDataImageTagOption
                {
                    // PropertyTagDateTime
                    Tag = ExifTag.DateTime
                }
            }
        };

        return serviceCollection.AddMetaDataParsing(options);
    }
    
    public static IServiceCollection AddMetaDataParsing(this IServiceCollection serviceCollection, MetaDataDateParserOptions options)
    {
        serviceCollection.AddSingleton(options);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // MetaDataDateParser uses windows specific apis
            serviceCollection.AddScoped<IMetaDataDateParser, WindowsMetaDataDateParser>();
        }
        else
        {
            serviceCollection.AddScoped<IMetaDataDateParser, OsAgnosticMetaDataParser>();
        }

        return serviceCollection;
    }
}