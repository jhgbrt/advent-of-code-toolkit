using Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;
using System.Reflection.PortableExecutable;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Show some stats from the configured private leaderboard. ")]
class Leaderboard : AsyncCommand<Leaderboard.Settings>
{
    private readonly IReportManager manager;
    private readonly IInputOutputService io;
    private readonly AoCLogic logic;
    public Leaderboard(IReportManager manager, IInputOutputService io, AoCLogic logic)
    {
        this.manager = manager;
        this.io = io;
        this.logic = logic;
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
        [Description("get the overall, all time leaderboard")]
        [CommandOption("-a|--alltimes")]
        public bool all { get; set; }
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

        var tasks = (
            from y in (options.all ? logic.Years() : Enumerable.Repeat(year, 1))
            select manager.GetLeaderboardAsync(y, id)
        ).ToArray();
        await Task.WhenAll(tasks);
        var entries = tasks.SelectMany(t => t.Result);

        var q = from e in entries
                group e by e.name into g
                select new LeaderboardEntry(g.Key, 0, g.Sum(x => x.score), g.Sum(x => x.stars), g.Max(g => g.lastStar)) into e
                orderby e.score descending, e.stars descending
                select e;

        var table = q.ToTable();
        io.Write(table);
                          

        return 0;
    }
}

