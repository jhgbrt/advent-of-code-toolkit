namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

using System.ComponentModel;

[Description("Initialize the code for a specific puzzle. Requires AOC_SESSION set as an environment variable")]
class Init : SinglePuzzleCommand<Init.Settings>
{
    private readonly ICodeManager manager;
    private readonly IInputOutputService output;

    public Init(ICodeManager manager, AoCLogic logic, IInputOutputService output) : base(logic)
    {
        this.manager = manager;
        this.output = output;
    }
    public class Settings : AoCSettings
    {
        [property: Description("Force (if true, refresh cache)")]
        [CommandOption("-f|--force")]
        public bool? force { get; set; }
    }
    public override async Task<int> ExecuteAsync(int year, int day, Settings options)
    {
        var force = options.force ?? false;
        output.WriteLine("Puzzle is unlocked");
        await manager.InitializeCodeAsync(year, day, force, output.WriteLine);
        return 0;
    }



}


