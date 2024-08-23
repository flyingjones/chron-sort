using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Services.DateParser.MetaData;

public static partial class MetaDataParserServiceCollectionExtension
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
                    TagId = 0x9003, ParserFund = ParseDateFromTag
                },
                new MetaDataImageTagOption
                {
                    // PropertyTagExifDTDigitized
                    TagId = 0x9004, ParserFund = ParseDateFromTag
                },
                new MetaDataImageTagOption
                {
                    // PropertyTagDateTime
                    TagId = 0x9003, ParserFund = ParseDateFromTag
                }
            }
        };

        return serviceCollection.AddMetaDataParsing(options);
    }
    
    public static IServiceCollection AddMetaDataParsing(this IServiceCollection serviceCollection, MetaDataDateParserOptions options)
    {
        serviceCollection.AddSingleton(options);
        serviceCollection.AddScoped<IMetaDataDateParser, MetaDataDateParser>();

        return serviceCollection;
    }

    private static DateTime? ParseDateFromTag(string dateString)
    {
        var cleanedUpDate = DotRegex().Replace(dateString, "-", 2);
        if (DateTime.TryParse(cleanedUpDate, out var result))
        {
            return result;
        }

        return null;
    }
    
    [GeneratedRegex(":")]
    private static partial Regex DotRegex();
}