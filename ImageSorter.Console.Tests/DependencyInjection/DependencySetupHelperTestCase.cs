using ImageSorter.DependencyInjection;
using ImageSorter.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Test.Utils;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Tests.DependencyInjection;

public class DependencySetupHelperTestCase : AbstractTestCase
{
    public bool IsDryRun { get; set; }
    
    public bool MoveFiles { get; set; }
    
    public bool ScanParallel { get; set; }
    
    public bool UseProgressBar { get; set; }
    
    public DestinationConflictMode DestinationConflictMode { get; set; }
    
    public ConflictReducerMode ConflictReducerMode { get; set; }
    
    public ConflictResolverMode ConflictResolverMode { get; set; }

    public RunConfiguration ToRunConfiguration()
    {
        return new RunConfiguration
        {
            SortConfiguration = null,
            PreferFileNameParsing = false,
            SourcePath = new FileInfo(TestContext.CurrentContext.TestDirectory),
            DestinationPath = new FileInfo(TestContext.CurrentContext.TestDirectory),
            SummaryFilePath = null,
            SummaryFileDirectoryPath = null,
            EscapeSummaryFileTables = false,
            MoveFiles = MoveFiles,
            FileEndings = null,
            From = null,
            To = null,
            ScanParallel = ScanParallel,
            LogLevel = LogLevel.Trace,
            SkipParserBefore = default,
            SkipParserAfter = default,
            IsDryRun = IsDryRun,
            OutputFormat = null,
            UseProgressBar = UseProgressBar,
            ProgressBarCharacters = null,
            ConflictReducerMode = ConflictReducerMode,
            DestinationConflictMode = DestinationConflictMode,
            ConflictResolverMode = ConflictResolverMode,
            CaseSensitivityDetectionMode = CaseSensitivityDetectionMode.Insensitive,
            FileSystemIsCaseSensitive = false
        };
    }
}