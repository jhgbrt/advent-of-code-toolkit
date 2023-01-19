
using Net.Code.AdventOfCode.Toolkit.Core;

using static Microsoft.CodeAnalysis.AssemblyIdentityComparer;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class ReportManager : IReportManager
{
    private readonly IPuzzleManager manager;
    public ReportManager(IPuzzleManager manager)
    {
        this.manager = manager;
    }


    public async Task<IEnumerable<PuzzleReportEntry>> GetPuzzleReport(ResultStatus? status, int? slowerthan, int? year)
    {

        return from item in await manager.GetPuzzleResults(year, slowerthan.HasValue ? TimeSpan.FromSeconds(slowerthan.Value) : null)
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
    }
}