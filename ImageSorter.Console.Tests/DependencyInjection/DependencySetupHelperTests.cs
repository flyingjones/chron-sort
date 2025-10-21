using ImageSorter.DependencyInjection;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Test.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Tests.DependencyInjection;

[TestFixture]
public class DependencySetupHelperTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void SetupServices_NoMissingDependencies(DependencySetupHelperTestCase testCase)
    {
        // arrange
        var serviceCollection = testCase.ToRunConfiguration().SetupServices();
        // initialized in the main and not the normal service stuff
        serviceCollection.AddSingleton<IMarkdownFileWriter, MockMarkdownFileWriter>();
        
        // act & assert
        Assert.DoesNotThrow(() =>
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            // top level dependencies which are constructed in the main
            var logger = serviceProvider.GetRequiredService<ILogger<DependencySetupHelperTests>>();
            var markDownRenderEngine = serviceProvider.GetRequiredService<IMarkdownTableRenderEngine>();
            var sorter = serviceProvider.GetRequiredService<ISorter>();
        });
    }

    private static Func<DependencySetupHelperTestCase> BaseTestCaseFactory => () =>
        new DependencySetupHelperTestCase
        {
            IsDryRun = false,
            MoveFiles = false,
            ScanParallel = false,
            UseProgressBar = false,
            DestinationConflictMode = DestinationConflictMode.Skip,
            ConflictReducerMode = ConflictReducerMode.None,
            ConflictResolverMode = ConflictResolverMode.Throw
        };
    
    private static IEnumerable<DependencySetupHelperTestCase> GetTestCases()
    {
        yield return BaseTestCaseFactory();

        yield return BaseTestCaseFactory
            .ConfigureProperty(x => x.IsDryRun, true);
        yield return BaseTestCaseFactory
            .ConfigureProperty(x => x.MoveFiles, true);
        yield return BaseTestCaseFactory
            .ConfigureProperty(x => x.ScanParallel, true);
        yield return BaseTestCaseFactory
            .ConfigureProperty(x => x.UseProgressBar, true);

        foreach (var testCase in BaseTestCaseFactory
                     .ConfigureEnum(x => x.DestinationConflictMode))
        {
            yield return testCase;
        }
        
        foreach (var testCase in BaseTestCaseFactory
                     .ConfigureEnum(x => x.ConflictResolverMode))
        {
            yield return testCase;
        }
        
        foreach (var testCase in BaseTestCaseFactory
                     .ConfigureEnum(x => x.ConflictReducerMode))
        {
            yield return testCase;
        }
    }
}