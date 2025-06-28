using System.CommandLine;
using ImageSorter;

Console.CursorVisible = false;
var rootCommand = RootCommandFactory.BuildRootCommand();
rootCommand.SetHandler(RootCommandHandler.Handle);
await rootCommand.InvokeAsync(args);

// TODO to error handling (e.g. catch some exceptions and print a semi pretty error message)

Thread.Sleep(50);