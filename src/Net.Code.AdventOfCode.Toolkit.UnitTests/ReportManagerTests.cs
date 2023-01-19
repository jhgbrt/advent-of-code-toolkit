using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class ReportManagerTests
{
    [Fact]
    public async Task GetPuzzleReportTest()
    {
        var manager = Substitute.For<IPuzzleManager>();
        var rm = new ReportManager(manager);
        var report = await rm.GetPuzzleReport(null, null, null);
        await manager.Received().GetPuzzleResults(null, null);
    }

    [Fact]
    public async Task GetMemberStatsTest()
    {
        var clock = TestClock.Create(2017, 1, 1, 0, 0, 0);
        var client = Substitute.For<IAoCClient>();
        var manager = Substitute.For<IPuzzleManager>();

#pragma warning disable CS8619
        Task<Member?> task = Task.FromResult(
            new Member(1, "", 0, 0, 0, clock.GetCurrentInstant(), new Dictionary<int, DailyStars>())
            );

        client.GetMemberAsync(Arg.Any<int>())
            .Returns(task);

        var logic = new AoCLogic(clock);
        var rm = new MemberManager(client);

        var report = await rm.GetMemberStats(logic.Years()).ToListAsync();

        Assert.Equal(3, report.Count);

    }
}