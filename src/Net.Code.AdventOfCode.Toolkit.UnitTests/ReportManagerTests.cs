using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class ReportManagerTests
{
    [Fact]
    public async Task GetPuzzleReportTest()
    {
        var client = Substitute.For<IAoCClient>();
        var manager = Substitute.For<IPuzzleManager>();

        manager.GetPuzzleResult(Arg.Any<int>(), Arg.Any<int>())
            .Returns(callInfo => new PuzzleResultStatus(
                new Puzzle(callInfo.ArgAt<int>(0), callInfo.ArgAt<int>(1), "", "", "", Answer.Empty, Status.Unlocked),
                DayResult.NotImplemented(callInfo.ArgAt<int>(0), callInfo.ArgAt<int>(1)))
            );

        var rm = new ReportManager(client, manager, new AoCLogic(TestClock.Create(2017,1,1,0,0,0)));

        var report = await rm.GetPuzzleReport(null, null).ToListAsync();

        Assert.Equal(50, report.Count);
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

        client.GetMemberAsync(Arg.Any<int>(), true)
            .Returns(task);

        var rm = new ReportManager(client, manager, new AoCLogic(clock));

        var report = await rm.GetMemberStats().ToListAsync();

        Assert.Equal(3, report.Count);

    }
}