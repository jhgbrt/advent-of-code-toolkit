
using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class ReportManager : IReportManager
{
    private readonly IPuzzleManager manager;
    public ReportManager(IPuzzleManager manager)
    {
        this.manager = manager;
    }


    public async IAsyncEnumerable<PuzzleReportEntry> GetPuzzleReport(ResultStatus? status, int? slowerthan, IEnumerable<(int, int)> puzzles)
    {
        var item = manager.GetPuzzleResult(2018, 21);
        
        var results = (await manager.GetPuzzleResults(slowerthan)).ToDictionary(r => r.Key);

        foreach (var (year, day) in puzzles)
        {
            var puzzle = await manager.GetPuzzle(year, day);
            var p = new PuzzleResultStatus(puzzle, results.GetValueOrDefault(new(year,day), DayResult.NotImplemented(year,day)));

            var comparisonResult = p.puzzle.Compare(p.result);

            if (status.HasValue && (comparisonResult.part1 != status.Value || comparisonResult.part2 != status.Value)) continue;
            if (slowerthan.HasValue && p.result.Elapsed < TimeSpan.FromSeconds(slowerthan.Value)) continue;

            yield return new PuzzleReportEntry(
                p.puzzle.Year,
                p.puzzle.Day,
                p.puzzle.Answer.part1,
                p.puzzle.Answer.part2,
                p.result.Part1.Value,
                p.result.Part1.Elapsed,
                comparisonResult.part1,
                p.result.Part2.Value,
                p.result.Part2.Elapsed,
                comparisonResult.part2,
                p.result.Elapsed
                );
        }
    }
}