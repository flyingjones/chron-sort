using System.CommandLine.Parsing;

namespace ImageSorter;

public class Programm
{
    private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
    
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = RootCommandFactory.BuildRootCommand();
        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count == 0)
        {
            try
            {
                // create a cancellation token source to handle cancel key presses (CTRL + C
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    // Console.WriteLine("Cancel event triggered");
                    CancellationTokenSource.Cancel();
                    eventArgs.Cancel = true;
                };

                await RootCommandHandler.Handle(parseResult, CancellationTokenSource.Token);
            }
            finally
            {
                // here so all log statements make it to the console
                Thread.Sleep(50);
            }

            return 0;
        }
        else
        {
            foreach (ParseError parseError in parseResult.Errors)
            {
                await Console.Error.WriteLineAsync(parseError.Message);
            }

            return 1;
        }
    }
}