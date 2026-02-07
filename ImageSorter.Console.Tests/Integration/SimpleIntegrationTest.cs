using ImageSorter.DependencyInjection;
using ImageSorter.FileHandling.CaseSensitivity;
using ImageSorter.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Tests.InMemory;
using ImageSorter.Tests.InMemory.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Tests.Integration;

[TestFixture]
public class SimpleIntegrationTest
{
    [Test]
    public async Task SimpleTest()
    {
        // arrange
        var fs = new InMemoryFileSystem();

        fs.CreateDirectory("/images/share");
        fs.CreateDirectory("/images/phone");
        var filesToAdd = new[]
        {
            new KeyValuePair<string, DateTime>("/images/share/img01.jpg", DateTime.Parse("2025-01-01")),
            new KeyValuePair<string, DateTime>("/images/phone/img02.jpg", DateTime.Parse("2025-02-01"))
        };
        AddEmptyFiles(fs, filesToAdd);

        var runConfig = new RunConfiguration
        {
            SortConfiguration = [],
            PreferFileNameParsing = false,
            SourcePath = InMemoryFileSystem.NormalizePath("/images"),
            DestinationPath = InMemoryFileSystem.NormalizePath("/out"),
            SummaryFilePath = null,
            SummaryFileDirectoryPath = null,
            EscapeSummaryFileTables = false,
            MoveFiles = false,
            ScanParallel = false,
            SkipParserBefore = default,
            SkipParserAfter = default,
            IsDryRun = false,
            OutputFormat = "yyyy/MM/dd",
            UseProgressBar = false,
            ProgressBarCharacters = "#",
            ConflictReducerMode = ConflictReducerMode.None,
            DestinationConflictMode = DestinationConflictMode.Joint,
            ConflictResolverMode = ConflictResolverMode.Throw,
            CaseSensitivityDetectionMode = CaseSensitivityDetectionMode.Sensitive
        };
        var serviceProvider = IntegrationTestSetupHelper.BuildIntegrationTestServices(runConfig, fs);

        var sorter = serviceProvider.GetRequiredService<ISorter>();
        
        // act
        await sorter.PerformSorting(CancellationToken.None);
        
        // assert
        var resultingFiles = fs.GetFiles("/out", "*", SearchOption.AllDirectories);
        Assert.That(resultingFiles, Has.Length.EqualTo(2));
        Assert.That(resultingFiles, Contains.Item(InMemoryFileSystem.NormalizePath("/out/2025/01/01/img01.jpg")));
        Assert.That(resultingFiles, Contains.Item(InMemoryFileSystem.NormalizePath("/out/2025/02/01/img02.jpg")));
    }

    private static void AddEmptyFiles(
        InMemoryFileSystem fs,
        ICollection<KeyValuePair<string, DateTime>> filesWithDate)
    {
        foreach (var pair in filesWithDate)
        {;
            fs.AddFile(pair.Key, [], pair.Value);
        }
    }
}