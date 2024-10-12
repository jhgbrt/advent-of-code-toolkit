using Net.Code.AdventOfCode.Toolkit.Commands;
using Net.Code.AdventOfCode.Toolkit.Core;

using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using NodaTime;

using NSubstitute;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class CommandTests
{
    const int Year = 2021;
    const int Day = 26;
    public CommandTests()
    {
        Clock = TestClock.Create(Year, 12, Day, 0, 0, 0);
        AoCLogic = new AoCLogic(Clock);
    }
    IClock Clock;
    AoCLogic AoCLogic;
    [Fact]
    public async Task Init()
    {
        var manager = CreateCodeManager();
        var puzzleManager = CreatePuzzleManager();
        var sut = new Init(puzzleManager, manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(new(Year, 1), new());
        await manager.Received(1).InitializeCodeAsync(Arg.Is<Puzzle>(p => p.Year == 2021 && p.Day == 1), false, null, Arg.Any<Action<string>>());
    }

    [Fact]
    public async Task Leaderboard_WithId()
    {
        var manager = CreateLeaderboardManager();
        var run = new Leaderboard(manager, Substitute.For<IInputOutputService>(), AoCLogic, Clock);
        await run.ExecuteAsync(new CommandContext([], Substitute.For<IRemainingArguments>(), "leaderboard", default), new Leaderboard.Settings { year = 2021, id = 123 });
        await manager.Received(1).GetLeaderboardAsync(123, Year);

    }

    [Fact]
    public async Task Leaderboard_NoId()
    {
        var manager = CreateLeaderboardManager();
        var run = new Leaderboard(manager, Substitute.For<IInputOutputService>(), AoCLogic, Clock);
        await run.ExecuteAsync(new CommandContext([], Substitute.For<IRemainingArguments>(), "leaderboard", default), new Leaderboard.Settings { year = 2021 });
        await manager.Received(1).GetLeaderboardAsync(123, Year);
    }
    [Fact]
    public async Task Leaderboard_All()
    {
        var manager = CreateLeaderboardManager();
        var run = new Leaderboard(manager, Substitute.For<IInputOutputService>(), AoCLogic, Clock);
        await run.ExecuteAsync(new CommandContext([], Substitute.For<IRemainingArguments>(), "leaderboard", default), new Leaderboard.Settings { year = 2021, all = true });
        await manager.Received().GetLeaderboardsAsync(123, Arg.Any<IEnumerable<int>>());
    }

    [Fact]
    public async Task Run()
    {
        var manager = Substitute.For<IAoCRunner>();
        var puzzleManager = CreatePuzzleManager();
        var key = new PuzzleKey(2021, 1);
        manager.Run(null, key, Arg.Any<Action<int, Result>>()).Returns(DayResult.NotImplemented(key));
        var run = new Run(manager, puzzleManager, AoCLogic, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(new(2021, 1), new());
        await manager.Received(1).Run(null, new(2021, 1), Arg.Any<Action<int, Result>>());
    }
    [Fact]
    public async Task Verify()
    {
        IPuzzleManager manager = CreatePuzzleManager();
        var run = new Verify(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(new(Year, 1), new());
        await manager.Received(1).GetPuzzleResult(new(Year, 1));
    }

    [Fact]
    public async Task Sync()
    {
        var manager = CreateCodeManager();
        var puzzleManager = CreatePuzzleManager();
        var sut = new Sync(puzzleManager, manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(new(Year, 1), new());
        await manager.Received(1).SyncPuzzleAsync(Arg.Is<Puzzle>(p => p.Year == 2021 && p.Day == 1));
    }

    [Fact]
    public async Task Export_NoOutput()
    {
        var manager = CreateCodeManager();
        var sut = new Export(manager, AoCLogic, Substitute.For<IInputOutputService>());
        await sut.ExecuteAsync(new(Year, 1), new());
        await manager.Received(1).GenerateCodeAsync(new(Year, 1));
        await manager.DidNotReceive().ExportCode(Arg.Any<PuzzleKey>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Export_Output()
    {
        var manager = CreateCodeManager();
        var sut = new Export(manager, AoCLogic, Substitute.For<IInputOutputService>());
        PuzzleKey key = new(Year, 1);
        await sut.ExecuteAsync(key, new Export.Settings { output = "output.txt" });
        await manager.Received(1).GenerateCodeAsync(key);
        await manager.Received(1).ExportCode(key, "public class AoC202101 {}", null, "output.txt");
    }
    [Fact]
    public async Task Export_Output_WithCommon()
    {
        var manager = CreateCodeManager();
        var sut = new Export(manager, AoCLogic, Substitute.For<IInputOutputService>());
        PuzzleKey key = new(Year, 1);
        var common = new[] { "file1", "file2" };
        await sut.ExecuteAsync(key, new Export.Settings { output = "output", includecommon = common });
        await manager.Received(1).GenerateCodeAsync(key);
        await manager.Received(1).ExportCode(key, "public class AoC202101 {}", common, "output");
    }

    [Fact]
    public async Task Post_WhenPuzzleIsValid()
    {
        var manager = CreatePuzzleManager();
        var sut = new Post(manager, AoCLogic, Substitute.For<IInputOutputService>());
        PuzzleKey key = new(Year, 5);
        await sut.ExecuteAsync(key, new Post.Settings { value = "SOLUTION" });
        await manager.Received().PostAnswer(key, "SOLUTION");
    }

    [Fact]
    public async Task Report()
    {
        var manager = CreatePuzzleManager();
        var run = new Report(manager, Substitute.For<IInputOutputService>());
        await run.ExecuteAsync(new CommandContext([], Substitute.For<IRemainingArguments>(), "report", default), new());
        await manager.Received().GetPuzzleResults(Arg.Any<int?>(), Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task Stats()
    {
        var manager = CreateLeaderboardManager();
        var run = new Stats(manager, Substitute.For<IInputOutputService>(), AoCLogic);
        await run.ExecuteAsync(new CommandContext([], Substitute.For<IRemainingArguments>(), "stats", default), default!);
        manager.Received().GetMemberStats(Arg.Any<IEnumerable<int>>());
    }

    private ILeaderboardManager CreateLeaderboardManager()
    {
        var manager = Substitute.For<ILeaderboardManager>();
        manager.GetLeaderboardIds(Year).Returns(new[] { (123, "") });
        return manager;
    }


    private ICodeManager CreateCodeManager()
    {
        var manager = Substitute.For<ICodeManager>();
        foreach (var key in AoCLogic.Puzzles())
            manager.GenerateCodeAsync(key).Returns(
                $"public class AoC{key.Year}{key.Day:00} {{}}"
                );
        return manager;
    }

    private IPuzzleManager CreatePuzzleManager()
    {
        var manager = Substitute.For<IPuzzleManager>();
        foreach (var key in AoCLogic.Puzzles())
        {
            var puzzle = Puzzle.Create(key, "input", Answer.Empty);
            var result = DayResult.NotImplemented(key);
            var status = new PuzzleResultStatus(puzzle, result);
            manager.GetPuzzle(key).Returns(puzzle);
            manager.SyncPuzzle(key).Returns(puzzle);
            manager.GetPuzzleResult(key).Returns(status);
        }

        return manager;
    }

}

public class PuzzleTests
{
    [Fact]
    public void CreateAnswer_WhenPuzzleIsLocked_Throws()
    {
        var puzzle = Puzzle.Locked(new(2015, 1));
        Assert.Throws<PuzzleLockedException>(() => puzzle.CreateAnswer("any"));
    }

    [Fact]
    public void CreateAnswer_WhenPuzzleIsCompleted_Throws()
    {
        var puzzle = new Puzzle(new(2015, 1), "input", new Answer("a", "b"), Status.Completed);
        Assert.Throws<AlreadyCompletedException>(() => puzzle.CreateAnswer("any"));
    }

    [Fact]
    public void CreateAnswer_WhenPuzzleIsUnlocked_ReturnsAnswerForPart1()
    {
        var puzzle = Puzzle.Create(new(2015, 1), "input", Answer.Empty);
        var answer = puzzle.CreateAnswer("answer");
        Assert.Equal((1, "answer"), (answer.part, answer.value));
    }

    [Fact]
    public void CreateAnswer_WhenPart1IsAnswered_ReturnsAnswerForPart2()
    {
        var puzzle = Puzzle.Create(new(2015, 1), "input", new Answer("part1", string.Empty));
        var answer = puzzle.CreateAnswer("answer");
        Assert.Equal((2, "answer"), (answer.part, answer.value));
    }
}