
using Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using Spectre.Console;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class ReportManager : IReportManager
{
    private readonly IAoCClient client;
    private readonly IPuzzleManager manager;
    private readonly AoCLogic AoCLogic;
    public ReportManager(IAoCClient client, IPuzzleManager manager, AoCLogic aocLogic)
    {
        this.client = client;
        this.manager = manager;
        this.AoCLogic = aocLogic;
    }

    public Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(bool usecache)
        => client.GetLeaderboardIds(usecache);

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int id)
    {
        var leaderboard = await client.GetLeaderBoardAsync(year, id, false);

        if (leaderboard == null)
        {
            return Enumerable.Empty<LeaderboardEntry>();
        }

        return from m in leaderboard.Members.Values
               let name = m.Name
               let score = m.LocalScore
               let stars = m.TotalStars
               let lastStar = m.LastStarTimeStamp
               where lastStar.HasValue && lastStar > Instant.MinValue
               let dt = lastStar.Value.InUtc().ToDateTimeOffset().ToLocalTime()
               orderby score descending
               select new LeaderboardEntry(name, year, score, stars, dt);
    }

    public async IAsyncEnumerable<(int year, MemberStats stats)> GetMemberStats()
    {
        foreach (var y in AoCLogic.Years())
        {
            var m = await client.GetMemberAsync(y);
            if (m == null) continue;
            yield return (y, new MemberStats(m.Name, m.TotalStars, m.LocalScore));
        }
    }

    public async IAsyncEnumerable<PuzzleReportEntry> GetPuzzleReport(ResultStatus? status, int? slowerthan)
    {
        foreach (var (year, day) in AoCLogic.Puzzles())
        {
            var p = await manager.GetPuzzleResult(year, day, (_, _) => { });
            var comparisonResult = p.puzzle.Compare(p.result);

            if (status.HasValue && (comparisonResult.part1 != status.Value || comparisonResult.part2 != status.Value)) continue;
            if (slowerthan.HasValue && p.result.Elapsed < TimeSpan.FromSeconds(slowerthan.Value)) continue;

            yield return new PuzzleReportEntry(
                p.puzzle.Year,
                p.puzzle.Day,
                p.puzzle.Answer.part1,
                p.puzzle.Answer.part2,
                p.result.part1.Value,
                p.result.part1.Elapsed,
                comparisonResult.part1,
                p.result.part2.Value,
                p.result.part2.Elapsed,
                comparisonResult.part2,
                p.result.Elapsed
                );
        }
    }
}