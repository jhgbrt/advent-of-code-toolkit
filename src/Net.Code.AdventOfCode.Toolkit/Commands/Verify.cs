namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using System.ComponentModel;

[Description("Verify the results for the given puzzle(s). Does not run the puzzle code.")]
class Verify(IPuzzleManager manager, AoCLogic aocLogic, IInputOutputService io) : ManyPuzzlesCommand<AoCSettings>(aocLogic)
{
    public override async Task<int> ExecuteAsync(PuzzleKey key, AoCSettings options)
    {
        var resultStatus = await manager.GetPuzzleResult(key);
        var reportLine = resultStatus.ToReportLineMarkup();
        io.MarkupLine(reportLine);
        return resultStatus.Ok ? 0 : 1;
    }

}

