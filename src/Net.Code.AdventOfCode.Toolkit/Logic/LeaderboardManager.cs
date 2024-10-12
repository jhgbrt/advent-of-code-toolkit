
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Core.Leaderboard;

using NodaTime;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class LeaderboardManager(IAoCClient client) : ILeaderboardManager
{
    public Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(int year)
          => client.GetLeaderboardIds(year);

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardsAsync(int id, IEnumerable<int> years)
    {
        var tasks = (
            from y in years
            select GetLeaderboardAsync(id, y)
        ).ToArray();
        await Task.WhenAll(tasks);
        var entries = tasks.SelectMany(t => t.Result);
        return entries;
    }
    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int id, int year)
    {
        var leaderboard = await client.GetLeaderBoardAsync(year, id);

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
    public async IAsyncEnumerable<MemberStats> GetMemberStats(IEnumerable<int> years)
    {
        foreach (var y in years)
        {
            var m = await client.GetPersonalStatsAsync(y);
            if (m == null) continue;
            yield return new MemberStats(y, m.Name, m.TotalStars, m.LocalScore);
        }
    }

}
