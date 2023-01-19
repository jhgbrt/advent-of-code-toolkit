namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

using System.ComponentModel;

[Description("Initialize the code for a specific puzzle. Requires AOC_SESSION set as an environment variable")]
class Init : SinglePuzzleCommand<Init.Settings>
{
    private readonly IPuzzleManager puzzleManager;
    private readonly ICodeManager codeManager;
    private readonly IInputOutputService output;

    public Init(IPuzzleManager puzzleManager, ICodeManager codeManager, AoCLogic logic, IInputOutputService output) : base(logic)
    {
        this.puzzleManager = puzzleManager;
        this.codeManager = codeManager;
        this.output = output;
    }
    public class Settings : AoCSettings
    {
        [property: Description("Force (if true, refresh cache)")]
        [CommandOption("-f|--force")]
        public bool? force { get; set; }
    }
    public override async Task<int> ExecuteAsync(PuzzleKey key, Settings options)
    {
        var force = options.force ?? false;
        output.WriteLine($"The puzzle for {key} is unlocked; initializing code...");
        var puzzle = await puzzleManager.GetPuzzle(key);
        await codeManager.InitializeCodeAsync(puzzle, force, output.WriteLine);
        return 0;
    }



}


