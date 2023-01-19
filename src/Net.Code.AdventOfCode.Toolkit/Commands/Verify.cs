
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

    public override async Task<int> ExecuteAsync(PuzzleKey key, AoCSettings options)
    {
        var resultStatus = await manager.GetPuzzleResult(key);
        var reportLine = resultStatus.ToReportLineMarkup();
        io.MarkupLine(reportLine);
        return resultStatus.Ok ? 0 : 1;
    }

}