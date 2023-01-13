
using Net.Code.AdventOfCode.Toolkit.Core;

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
        foreach (var (year, day) in puzzles)
        {
            var puzzle = await manager.GetPuzzle(year, day);
            var result = await manager.GetPuzzleResult(year, day);
            var p = new PuzzleResultStatus(puzzle, result);
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