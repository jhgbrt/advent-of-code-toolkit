namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;

[Description("Show a list of all puzzles, their status (unlocked, answered), and the answers posted.")]
class Report : AsyncCommand<Report.Settings>
{
    private readonly IReportManager manager;
    private readonly IInputOutputService io;
    private readonly AoCLogic logic;

    public Report(IReportManager manager, IInputOutputService io, AoCLogic logic)
    {
        this.manager = manager;
        this.io = io;
        this.logic = logic;
    }
    public class Settings : CommandSettings
    {
        [Description($"Filter by status. Valid values: {nameof(ResultStatus.NotImplemented)}, {nameof(ResultStatus.AnsweredButNotImplemented)}, {nameof(ResultStatus.Failed)}")]
        [CommandOption("--status")]
        public ResultStatus? status { get; set; }
        [Description("Only include entries for puzzles that take longer than the indicated number of seconds")]
        [CommandOption("--slower-than")]
        public int? slowerthan { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings options)
    {
        var report = await manager.GetPuzzleReport(options.status, options.slowerthan, logic.Puzzles()).ToListAsync();
        io.Write(report.ToTable());
        return 0;
    }

}

static class TableFactory
{
    public static Table ToTable(this IEnumerable<LeaderboardEntry> entries)
    {
        var table = new Table();
        table.AddColumns("rank", "member", "points", "stars", "lastStar");

        int n = 1;
        foreach (var line in entries)
        {
            table.AddRow(
                n.ToString(),
                line.name,
                line.score.ToString(),
                line.stars.ToString(),
                line.lastStar.ToLocalTime().ToString("dd MMM yyyy HH:mm:ss") ?? string.Empty
                );
            n++;
        }
        return table;
    }
    public static Table ToTable(this IEnumerable<PuzzleReportEntry> report)
    {
        var table = new Table();

        table.AddColumns(
            new TableColumn(nameof(PuzzleReportEntry.year)).RightAligned(),
            new TableColumn(nameof(PuzzleReportEntry.day)).RightAligned(),
            new TableColumn(nameof(PuzzleReportEntry.answer1)).Width(20),
            new TableColumn(nameof(PuzzleReportEntry.elapsed1)).RightAligned(),
            new TableColumn(nameof(PuzzleReportEntry.status1)).RightAligned(),
            new TableColumn(nameof(PuzzleReportEntry.answer2)).Width(20),
            new TableColumn(nameof(PuzzleReportEntry.elapsed2)).RightAligned(),
            new TableColumn(nameof(PuzzleReportEntry.status2)).RightAligned(),
            new TableColumn(nameof(PuzzleReportEntry.elapsedTotal)).RightAligned()
            );

        foreach (var item in report)
        {
            table.AddRow(
                item.year.ToString(),
                item.day.ToString(),
                Format(item.answer1, item.result1),
                Format(item.elapsed1),
                Format(item.status1),
                Format(item.answer2, item.result2),
                Format(item.elapsed2),
                Format(item.status2),
                Format(item.elapsedTotal)
                );
        }
        return table;
    }

    static string Format(string answer, string result) => answer switch
    {
        "" => $"[yellow]{result}[/]",
        _ when answer.Equals(result) => answer,
        _ => $"[red][strikethrough]{result}[/] {answer}[/]"
    };
    static string Format(ResultStatus status)
    {
        return status switch
        {
            ResultStatus.Ok => $"[green]{status.GetDisplayName()}[/]",
            ResultStatus.Failed => $"[red]{status.GetDisplayName()}[/]",
            ResultStatus.NotImplemented => $"[yellow]{status.GetDisplayName()}[/]",
            ResultStatus.AnsweredButNotImplemented => $"[red]{status.GetDisplayName()}[/]",
            ResultStatus.Unknown => $"[yellow]{status.GetDisplayName()}[/]",
            _ => status.GetDisplayName()
        };
    }
    static string Format(TimeSpan t)
    {
        return t switch
        {
            { TotalSeconds: < 1 } => $"[green]{t.Milliseconds} ms[/]",
            { TotalMinutes: < 1 } => $@"[yellow]{t:s\.f} s[/]",
            { TotalHours: < 1 } => $@"[red]{t:mm\:ss} m[/]",
            _ => $@"[red]{t} m[/]"
        };
    }
}
