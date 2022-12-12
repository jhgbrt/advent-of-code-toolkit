
using Net.Code.AdventOfCode.Toolkit.Core;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Verify the results for the given puzzle(s). Does not run the puzzle code.")]
class Verify : ManyPuzzlesCommand<AoCSettings>
{
    private readonly IPuzzleManager manager;
    private readonly IInputOutputService io;

    public Verify(IPuzzleManager manager, AoCLogic aocLogic, IInputOutputService io) : base(aocLogic)
    {
        this.manager = manager;
        this.io = io;
    }

    public override async Task<int> ExecuteAsync(int year, int day, AoCSettings options)
    {
        var resultStatus = await manager.GetPuzzleResult(year, day, (part, result) => io.MarkupLine($"{year}-{day:00} part {part}: {result.Value} ({result.Elapsed})"));
        var reportLine = resultStatus.ToReportLineMarkup();
        io.MarkupLine(reportLine.ToString());
        if (!resultStatus.Ok)
            return 1;
        return 0;
    }

}



