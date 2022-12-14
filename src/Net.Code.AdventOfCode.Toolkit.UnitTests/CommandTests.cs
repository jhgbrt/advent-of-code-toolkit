using Net.Code.AdventOfCode.Toolkit.Commands;
using Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using NSubstitute;

using Spectre.Console.Cli;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class CommandTests
{
    public CommandTests()
    {
        Clock = TestClock.Create(2021, 12, 26, 0, 0, 0);
        AoCLogic = new AoCLogic(Clock);
    }
    IClock Clock;
    AoCLogic AoCLogic;
    [Fact]
    public async Task Init()
    {
        var manager = CreateCodeManager();
        var sut = new Init(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(2021, 1, new());
        await manager.Received(1).InitializeCodeAsync(2021, 1, false, Arg.Any<Action<string>>());
    }

    [Fact]
    public async Task Leaderboard_WithId()
    {
        var manager = CreateReportManager();
        var run = new Leaderboard(manager, Substitute.For<IInputOutputService>(), AoCLogic);
        await run.ExecuteAsync(new CommandContext(Substitute.For<IRemainingArguments>(), "leaderboard", default), new Leaderboard.Settings { year = 2021, id = 123 });
        await manager.Received(1).GetLeaderboardAsync(2021, 123);

    }

    [Fact]
    public async Task Leaderboard_NoId()
    {
        var manager = CreateReportManager();
        var run = new Leaderboard(manager, Substitute.For<IInputOutputService>(), AoCLogic);
        await run.ExecuteAsync(new CommandContext(Substitute.For<IRemainingArguments>(), "leaderboard", default), new Leaderboard.Settings { year = 2021 });
        await manager.Received(1).GetLeaderboardAsync(2021, 123);
    }
    [Fact]
    public async Task Leaderboard_All()
    {
        var manager = CreateReportManager();
        var run = new Leaderboard(manager, Substitute.For<IInputOutputService>(), AoCLogic);
        await run.ExecuteAsync(new CommandContext(Substitute.For<IRemainingArguments>(), "leaderboard", default), new Leaderboard.Settings { year = 2021, all = true });
        await manager.Received(7).GetLeaderboardAsync(Arg.Is<int>(i => i <= 2021 && i >= 2015), 123);
    }

    [Fact]
    public async Task Run()
    {
        var manager = Substitute.For<IAoCRunner>();
        IPuzzleManager puzzleManager = CreatePuzzleManager();
        var run = new Run(manager, puzzleManager, AoCLogic, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(2021, 1, new());
        await manager.Received(1).Run(null, 2021, 1, Arg.Any<Action<int, Result>>());
    }
    [Fact]
    public async Task Verify()
    {
        IPuzzleManager manager = CreatePuzzleManager();
        var run = new Verify(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(2021, 1, new());
        await manager.Received(1).GetPuzzleResult(2021, 1, Arg.Any<Action<int, Result>>());
    }

    [Fact]
    public async Task Sync()
    {
        var manager = CreateCodeManager();
        var sut = new Sync(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(2021, 1, new());
        await manager.Received(1).SyncPuzzleAsync(2021, 1);
    }

    [Fact]
    public async Task Export_NoOutput()
    {
        var manager = CreateCodeManager();
        var sut = new Export(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(2021, 1, new());
        await manager.Received(1).GenerateCodeAsync(2021, 1);
        await manager.DidNotReceive().ExportCode(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Export_Output()
    {
        var manager = CreateCodeManager();
        var sut = new Export(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(2021, 1, new Export.Settings { output = "output.txt" });
        await manager.Received(1).GenerateCodeAsync(2021, 1);
        await manager.Received(1).ExportCode(2021, 1, "public class AoC202101 {}", false, "output.txt");
    }

    [Fact]
    public async Task Post_WhenPuzzleIsValid()
    {
        var manager = CreatePuzzleManager();
        manager.PreparePost(Arg.Any<int>(), Arg.Any<int>()).Returns((true, "reason", 1));
        var sut = new Post(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(2021, 5, new Post.Settings { value = "SOLUTION" });
        await manager.Received().Post(2021, 5, 1, "SOLUTION");
    }

    [Fact]
    public async Task Post_WhenPuzzleIsInvalid()
    {
        var manager = CreatePuzzleManager();
        manager.PreparePost(Arg.Any<int>(), Arg.Any<int>()).Returns((false, "reason", 0));
        var sut = new Post(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(2021, 5, new Post.Settings { value = "SOLUTION" });
        await manager.DidNotReceive().Post(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Report()
    {
        var manager = CreateReportManager();
        var run = new Report(manager, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(new CommandContext(Substitute.For<IRemainingArguments>(), "report", default), new() );
        manager.Received().GetPuzzleReport(default, default);
    }

    [Fact]
    public async Task Stats()
    {
        var manager = CreateReportManager();
        var run = new Stats(manager, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(new CommandContext(Substitute.For<IRemainingArguments>(), "stats", default), default!);
        manager.Received().GetMemberStats();
    }

    private IReportManager CreateReportManager()
    {
        var manager = Substitute.For<IReportManager>();
        manager.GetLeaderboardIds(Arg.Any<bool>()).Returns(new[] { (123, "") });
        return manager;
    }

    private ICodeManager CreateCodeManager()
    {
        var manager = Substitute.For<ICodeManager>();
        foreach (var y in AoCLogic.Years())
            foreach (var d in Enumerable.Range(1, 25))
                manager.GenerateCodeAsync(y, d).Returns(
                    $"public class AoC{y}{d:00} {{}}"
                    );
        return manager;
    }

    private IPuzzleManager CreatePuzzleManager()
    {
        var manager = Substitute.For<IPuzzleManager>();
        foreach (var y in AoCLogic.Years())
            foreach (var d in Enumerable.Range(1, 25))
                manager.GetPuzzleResult(y, d, Arg.Any<Action<int, Result>>()).Returns(
                    Task.FromResult(new PuzzleResultStatus(
                        new Puzzle(y, d, string.Empty, string.Empty, string.Empty, Answer.Empty, Status.Unlocked),
                        DayResult.NotImplemented(y, d)))
                    );

        return manager;
    }

}
