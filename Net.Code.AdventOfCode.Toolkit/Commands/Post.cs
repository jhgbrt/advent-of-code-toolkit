namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


[Description("Post an answer for a puzzle part. Requires AOC_SESSION set as an environment variable.")]
class Post : SinglePuzzleCommand<Post.Settings>
{
    private readonly IPuzzleManager manager;
    private readonly IInputOutputService io;

    public Post(IPuzzleManager manager, AoCLogic logic, IInputOutputService io) : base(logic)
    {
        this.manager = manager;
        this.io = io;
    }
    public class Settings : CommandSettings, IAoCSettings
    {

        [Description("The solution to the current puzzle part"), Required]
        [CommandArgument(0, "<SOLUTION>")]
        public string? value { get; set; }
        [Description("Year (default: current year)")]
        [CommandArgument(1, "[YEAR]")]
        public int? year { get; set; }
        [Description("Day (default: current day)")]
        [CommandArgument(2, "[DAY]")]
        public int? day { get; set; }

    }
    public override async Task<int> ExecuteAsync(int year, int day, Settings options)
    {
        (var status, var reason, var part) = await manager.PreparePost(year, day);
        if (!status)
        {
            io.WriteLine(reason);
            return 1;
        }
        var result = await manager.Post(year, day, part, options.value ?? string.Empty);

        var color = result.success ? Color.Green : Color.Red;
        io.MarkupLine($"[{color}]{result.content.EscapeMarkup()}[/]");
        return 0;
    }
}


