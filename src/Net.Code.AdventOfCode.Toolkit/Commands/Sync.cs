namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using System.ComponentModel;

[Description("Sync the data (specifically the posted answers) for a puzzle. Requires AOC_SESSION set as an environment variable.")]
class Sync(IPuzzleManager puzzleManager, ICodeManager codeManager, AoCLogic aocLogic, IInputOutputService io) : ManyPuzzlesCommand<AoCSettings>(aocLogic)
{
    public override async Task<int> ExecuteAsync(PuzzleKey key, AoCSettings _)
    {
        io.WriteLine($"Synchronizing for puzzle {key}...");
        var puzzle = await puzzleManager.SyncPuzzle(key);
        await codeManager.SyncPuzzleAsync(puzzle);
        return 0;
    }

}
