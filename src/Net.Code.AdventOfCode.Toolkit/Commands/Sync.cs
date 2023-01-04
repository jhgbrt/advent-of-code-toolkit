
using Net.Code.AdventOfCode.Toolkit.Core;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;


[Description("Sync the data (specifically the posted answers) for a puzzle. Requires AOC_SESSION set as an environment variable.")]
class Sync : ManyPuzzlesCommand<AoCSettings>
{
    private readonly ICodeManager manager;
    private readonly IInputOutputService io;

    public Sync(ICodeManager manager, AoCLogic aocLogic, IInputOutputService io) : base(aocLogic)
    {
        this.manager = manager;
        this.io = io;
    }

    public override async Task<int> ExecuteAsync(int year, int day, AoCSettings _)
    {
        io.WriteLine($"Synchronizing for puzzle {year}-{day:00}...");
        await manager.SyncPuzzleAsync(year, day);
        return 0;
    }

}
