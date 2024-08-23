using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Services.DateParser;

public static class DateParserServiceCollectionExtension
{
    public static IServiceCollection AddDateParsing(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDateParser, DateParser>();

        return serviceCollection;
    }
}