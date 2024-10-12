namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Spectre.Console.Cli;

using System.ComponentModel;

[Description("Initialize the code for a specific puzzle. Requires AOC_SESSION set as an environment variable")]
class Init(IPuzzleManager puzzleManager, ICodeManager codeManager, AoCLogic logic, IInputOutputService output) : SinglePuzzleCommand<Init.Settings>(logic)
{
    public class Settings : AoCSettings
    {
        [property: Description("Force (if true, refresh cache)")]
        [CommandOption("-f|--force")]
        public bool? force { get; set; }
        [property: Description("Template to use. Looks for subfolders under 'Template'. If not specified, the default template is assumed to be under Template")]
        [CommandOption("-t|--template")]
        public string? template{ get; set; }
    }
    public override async Task<int> ExecuteAsync(PuzzleKey key, Settings options)
    {
        var force = options.force ?? false;
        var template = options.template;
        output.WriteLine($"The puzzle for {key} is unlocked; initializing code using {template ?? "default"} template...");
        var puzzle = await puzzleManager.SyncPuzzle(key);
        await codeManager.InitializeCodeAsync(puzzle, force, options.template, output.WriteLine);
        return 0;
    }



}


