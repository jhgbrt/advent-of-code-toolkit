namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


[Description("Post an answer for a puzzle part. Requires AOC_SESSION set as an environment variable.")]
class Post(IPuzzleManager manager, AoCLogic logic, IInputOutputService io) : SinglePuzzleCommand<Post.Settings>(logic)
{
    public class Settings : CommandSettings, IAoCSettings
    {

        [Description("The solution to the current puzzle part"), Required]
        [CommandArgument(0, "<SOLUTION>")]
        public string value { get; set; } = string.Empty;
        [Description("Year (default: current year)")]
        [CommandArgument(1, "[YEAR]")]
        public int? year { get; set; }
        [Description("Day (default: current day)")]
        [CommandArgument(2, "[DAY]")]
        public int? day { get; set; }

    }
    public override async Task<int> ExecuteAsync(PuzzleKey key, Settings options)
    {
        var (success, content) = await manager.PostAnswer(key, options.value);

        var color = success ? Color.Green : Color.Red;
        io.MarkupLine($"[{color}]{content.EscapeMarkup()}[/]");
        return 0;
    }
}


