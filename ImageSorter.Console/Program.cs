using System.CommandLine;
using ImageSorter;

var rootCommand = RootCommandFactory.BuildRootCommand();
rootCommand.SetHandler(RootCommandHandler.Handle);
try
{
    // TODO to error handling (e.g. catch some exceptions and print a semi pretty error message)

    await rootCommand.InvokeAsync(args);
}
finally
{
    // here so all log statements make it to the console
    Thread.Sleep(50);
}