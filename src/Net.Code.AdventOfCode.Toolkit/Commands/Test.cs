
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Run the tests code for a specific puzzle.")]
class Test : ManyPuzzlesCommand<Test.Settings>
{
    private readonly IAoCRunner manager;
    private readonly IPuzzleManager puzzleManager;
    private readonly IInputOutputService io;

    public Test(IAoCRunner manager, IPuzzleManager puzzleManager, AoCLogic aocLogic, IInputOutputService io) : base(aocLogic)
    {
        this.manager = manager;
        this.puzzleManager = puzzleManager;
        this.io = io;
    }

    public class Settings : AoCSettings
    {
        [Description("The fully qualified name of the type containing the code for this puzzle. " +
        "Use a format string with {0} and {1} as placeholders for year and day. " +
        "(example: MyAdventOfCode.Year{0}.Day{1:00}.AoC{0}{1:00})")]
        [CommandOption("-t|--typename")]
        public string? typeName { get; set; }
    }

    public override async Task<int> ExecuteAsync(PuzzleKey key, Settings options)
    {
        var typeName = options.typeName;

        var puzzle = await puzzleManager.GetPuzzle(key);

        await manager.Test(typeName, key, (test, result) => io.MarkupLine($"test {test}: {result.Value} ({result.Elapsed})"));


        var result = await manager.Run(typeName, key, (part, result) => io.MarkupLine($"part {part}: {result.Value} ({result.Elapsed})"));
        if (result is not null)
        {
            var resultStatus = new PuzzleResultStatus(puzzle, result);
            var reportLine = resultStatus.ToReportLineMarkup();
            io.MarkupLine(reportLine);
        }

        return 0;
    }
}


