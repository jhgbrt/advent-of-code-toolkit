
using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Run the code for a specific puzzle.")]
class Run : ManyPuzzlesCommand<Run.Settings>
{
    private readonly IAoCRunner manager;
    private readonly IPuzzleManager puzzleManager;
    private readonly IInputOutputService io;

    public Run(IAoCRunner manager, IPuzzleManager puzzleManager, AoCLogic aocLogic, IInputOutputService io) : base(aocLogic)
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

    public override async Task<int> ExecuteAsync(int year, int day, Settings options)
    {
        var typeName = options.typeName;

        var puzzle = await puzzleManager.GetPuzzle(year, day);

        var result = await manager.Run(typeName, year, day, (part, result) => io.MarkupLine($"part {part}: {result.Value} ({result.Elapsed})"));

        await puzzleManager.SaveResult(result);

        var resultStatus = new PuzzleResultStatus(puzzle, result);

        var reportLine = resultStatus.ToReportLineMarkup();
        io.MarkupLine(reportLine);

        return 0;
    }
}


