using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

public static class RootCommandHandler
{
    /// <summary>
    /// Entrypoint into the program
    /// </summary>
    public static async Task Handle(InvocationContext context)
    {
        // parse arguments and build run config
        var parsedContext = context.ParseResult;
        var runConfiguration = RootCommandFactory.ParseRunConfiguration(parsedContext);
        
        // setup the service provider
        var serviceProvider = runConfiguration.SetupServices().BuildServiceProvider();
        
        // log the configuration
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        RunConfigurationHelper.LogRunConfiguration(logger, runConfiguration);

        // get the sorter and perform the sorting
        var sorter = serviceProvider.GetRequiredService<ISorter>();
        await sorter.PerformSorting(runConfiguration.MoveFiles, context.GetCancellationToken());
    }
}