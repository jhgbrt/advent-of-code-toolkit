namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using System.ComponentModel;

[Description("Sync the data (specifically the posted answers) for a puzzle. Requires AOC_SESSION set as an environment variable.")]
class Sync : ManyPuzzlesCommand<AoCSettings>
{
    private readonly IPuzzleManager puzzleManager;
    private readonly ICodeManager codeManager;
    private readonly IInputOutputService io;

    public Sync(IPuzzleManager puzzleManager, ICodeManager codeManager, AoCLogic aocLogic, IInputOutputService io) : base(aocLogic)
    {
        this.puzzleManager = puzzleManager;
        this.codeManager = codeManager;
        this.io = io;
    }

    public override async Task<int> ExecuteAsync(PuzzleKey key, AoCSettings _)
    {
        io.WriteLine($"Synchronizing for puzzle {key}...");
        var puzzle = await puzzleManager.SyncPuzzle(key);
        await codeManager.SyncPuzzleAsync(puzzle);
        return 0;
    }

}
