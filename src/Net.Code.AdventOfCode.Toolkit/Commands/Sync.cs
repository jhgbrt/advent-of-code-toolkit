﻿
using Net.Code.AdventOfCode.Toolkit.Core;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;


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

    public override async Task<int> ExecuteAsync(int year, int day, AoCSettings _)
    {
        io.WriteLine($"Synchronizing for puzzle {year}-{day:00}...");
        var puzzle = await puzzleManager.GetPuzzle(year, day);
        await codeManager.SyncPuzzleAsync(puzzle);
        return 0;
    }

}
