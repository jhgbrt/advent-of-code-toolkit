using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Show some stats from the configured private leaderboard. ")]
class Leaderboard : AsyncCommand<Leaderboard.Settings>
{
    private readonly IReportManager manager;
    private readonly IInputOutputService io;

    public Leaderboard(IReportManager manager, IInputOutputService io)
    {
        this.manager = manager;
        this.io = io;
    }
    public class Settings : CommandSettings
    {
        [Description("Year (default: current year)")]
        [CommandArgument(0, "[YEAR]")]
        public int year { get; set; } = DateTime.Now.Year;
        [Description("ID (if not provided, the leaderboard ID is looked up)")]
        [CommandArgument(0, "[id]")]
        public int? id { get; set; }
        [Description("force update")]
        [CommandOption("-f|--force")]
        public bool force { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings options)
    {
        var year = options.year;

        var ids = options.id.HasValue
            ? Enumerable.Repeat((id: options.id.Value, description: ""), 1)
            : await manager.GetLeaderboardIds(!options.force);

        var id = ids.Count() switch
        {
            > 1 => io.Prompt(new SelectionPrompt<(int id, string description)>().Title("Which leaderboard?").AddChoices(ids.Select(x => (x.id, x.description.EscapeMarkup())))).id,
            1 => ids.Single().id,
            _ => throw new Exception("no leaderboards found")
        };

        IEnumerable<LeaderboardEntry> entries = await manager.GetLeaderboardAsync(year, id);

        var table = entries.ToTable();
        io.Write(table);

        return 0;
    }
}

