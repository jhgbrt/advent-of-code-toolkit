using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class LeaderboardManagerTests
{
    [Fact]
    public async Task GetMemberStatsTest()
    {
        var clock = TestClock.Create(2017, 1, 1, 0, 0, 0);
        var client = Substitute.For<IAoCClient>();
        var manager = Substitute.For<IPuzzleManager>();

#pragma warning disable CS8619
        Task<PersonalStats?> task = Task.FromResult(
            new PersonalStats(1, "", 0, 0, 0, clock.GetCurrentInstant(), new Dictionary<int, DailyStars>())
            );

        client.GetPersonalStatsAsync(Arg.Any<int>())
            .Returns(task);

        var logic = new AoCLogic(clock);
        var rm = new LeaderboardManager(client);

        var report = await rm.GetMemberStats(logic.Years()).ToListAsync();

        Assert.Equal(3, report.Count);

    }
}