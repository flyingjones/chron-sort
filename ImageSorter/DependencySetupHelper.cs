using ImageSorter.Logging;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter;

public static class DependencySetupHelper
{
    public static IServiceCollection SetupServices(this RunConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddMetaDataParsing();
        serviceCollection.AddSingleton<IFileNameDateParser>(
            new FilenameDateParser(".*(20[0-9]{2}|19[0-9]{2})-(0[0-9]|1[0-9])-(0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", 0));
        serviceCollection.AddSingleton<IFileNameDateParser>(
            new FilenameDateParser(".*(20[0-9]{2}|19[0-9]{2})(0[0-9]|1[0-9])(0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", 1));
        serviceCollection.AddDateParsing();
        serviceCollection.AddStopwatchLogger();
        serviceCollection.AddDestinationWriter(configuration.DestinationPath.FullName, configuration.Overwrite);
        return serviceCollection;
    }
}