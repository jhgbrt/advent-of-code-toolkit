namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using Spectre.Console.Cli;

using System.ComponentModel;

[Description("Show a list of all puzzles, their status (unlocked, answered), and the answers posted.")]
class Report(IPuzzleManager manager, IInputOutputService io) : AsyncCommand<Report.Settings>
{
    public class Settings : CommandSettings
    {
        [Description($"Filter by status. Valid values: {nameof(ResultStatus.NotImplemented)}, {nameof(ResultStatus.AnsweredButNotImplemented)}, {nameof(ResultStatus.Failed)}")]
        [CommandOption("--status")]
        public ResultStatus? status { get; set; }
        [Description("Only include entries for puzzles that take longer than the indicated number of seconds")]
        [CommandOption("--slower-than")]
        public int? slowerthan { get; set; }
        [Description("Only include entries for puzzles of this year")]
        [CommandOption("--year")]
        public int? year { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings options)
    {
        var (status, slowerthan, year) = (options.status, options.slowerthan, options.year);
       
        var report = from item in await manager.GetPuzzleResults(year, slowerthan.HasValue ? TimeSpan.FromSeconds(slowerthan.Value) : null)
                     let puzzle = item.puzzle
                     let result = item.result
                     orderby puzzle.Year, puzzle.Day
                     let comparisonResult = item.Comparison
                     where !status.HasValue || (comparisonResult.part1 == status.Value && comparisonResult.part2 == status.Value)
                     where !slowerthan.HasValue || result.Elapsed >= TimeSpan.FromSeconds(slowerthan.Value)
                     select new PuzzleReportEntry(
                         puzzle.Year,
                         puzzle.Day,
                         puzzle.Answer.part1,
                         puzzle.Answer.part2,
                         result.Part1.Value,
                         result.Part1.Elapsed,
                         comparisonResult.part1,
                         result.Part2.Value,
                         result.Part2.Elapsed,
                         comparisonResult.part2,
                         result.Elapsed
                         );

        io.Write(report.ToTable());
        return 0;
    }
}
