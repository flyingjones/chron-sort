using ImageSorter.DateParsing.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorting.DateParsing;

public static class DateParserServiceCollectionExtension
{
    public static IServiceCollection AddDateParsing(this IServiceCollection serviceCollection, DateParserConfiguration configuration)
    {
        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddSingleton<IDateParser, DateParser>();

        return serviceCollection;
    }
}