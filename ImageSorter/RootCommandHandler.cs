using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

public static class RootCommandHandler
{
    public static async Task Handle(InvocationContext context)
    {
        var parsedContext = context.ParseResult;

        var runConfiguration = RootCommandFactory.ParseRunConfiguration(parsedContext);
        
        var serviceProvider = runConfiguration.SetupServices().BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        RunConfigurationHelper.LogRunConfiguration(logger, runConfiguration);

        var sorter = serviceProvider.GetRequiredService<ISorter>();
        
        await sorter.PerformSorting(runConfiguration.MoveFiles, context.GetCancellationToken());
    }
}