namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console;

record PuzzleReportEntry(
    int year, int day, string answer1, string answer2,
    string result1, TimeSpan elapsed1, ResultStatus status1,
    string result2, TimeSpan elapsed2, ResultStatus status2,
    TimeSpan elapsedTotal);

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
