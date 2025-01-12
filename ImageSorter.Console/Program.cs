using System.CommandLine;
using ImageSorter;

Console.CursorVisible = false;
var rootCommand = RootCommandFactory.BuildRootCommand();
rootCommand.SetHandler(RootCommandHandler.Handle);
await rootCommand.InvokeAsync(args);

Thread.Sleep(50);