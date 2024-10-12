using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using NSubstitute;
using NSubstitute.Extensions;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class ManyPuzzlesCommandTest
{
    private static ManyPuzzlesCommand<AoCSettings> CreateSystemUnderTest(int year, int month, int day)
    {
        var clock = TestClock.Create(year, month, day, 0, 0, 0);
        var logic = new AoCLogic(clock);
        var sut = Substitute.ForPartsOf<ManyPuzzlesCommand<AoCSettings>>(logic);
        return sut;
    }
    private async Task DoTest(ManyPuzzlesCommand<AoCSettings> sut, AoCSettings options)
    {
        var context = new CommandContext([], Substitute.For<IRemainingArguments>(), "name", default);
        await sut.Configure().ExecuteAsync(Arg.Any<PuzzleKey>(), options);
        await sut.ExecuteAsync(context, options);
    }

    [Fact]
    public async Task NoYearNoDay_OutsideAdvent()
    {
        (var year, var month, var day) = (2017, 1, 1);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings();
        await DoTest(sut, options);

        await sut.Received(50).ExecuteAsync(Arg.Any<PuzzleKey>(), options);
    }


    [Fact]
    public async Task NoYearNoDay_OutsideAdventInDecember_RunsAllPuzzlesForCurrentYear()
    {
        (var year, var month, var day) = (2016, 12, 26);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings();
        await DoTest(sut, options);

        await sut.Received(25).ExecuteAsync(Arg.Is<PuzzleKey>(k => k.Year == 2016), options);
    }
    [Fact]
    public async Task NoYearNoDay_DuringAdvent_RunsPuzzleForCurrentDay()
    {
        (var year, var month, var day) = (2017, 12, 20);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings();
        await DoTest(sut, options);

        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2017,20)), options);
    }
    [Fact]
    public async Task YearNoDay_OutsideAdvent()
    {
        (var year, var month, var day) = (2017, 1, 1);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2016 };
        await DoTest(sut, options);

        await sut.Received(25).ExecuteAsync(Arg.Is<PuzzleKey>(k => k.Year == 2016), options);
    }
    [Fact]
    public async Task YearNoDay_OutsideAdventInDecember_RunsAllPuzzlesForCurrentYear()
    {
        (var year, var month, var day) = (2016, 12, 26);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2016 };
        await DoTest(sut, options);

        await sut.Received(25).ExecuteAsync(Arg.Is<PuzzleKey>(k => k.Year == 2016), options);
    }
    [Fact]
    public async Task YearNoDay_DuringAdvent_ForPastYear_RunsAllPuzzlesForThatYear()
    {
        (var year, var month, var day) = (2017, 12, 20);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2016 };
        await DoTest(sut, options);

        await sut.Received(25).ExecuteAsync(Arg.Is<PuzzleKey>(k => k.Year == 2016), options);
    }
    [Fact]
    public async Task YearNoDay_DuringAdvent_ForCurrentYear_RunsAllPuzzlesForCurrentYear()
    {
        (var year, var month, var day) = (2017, 12, 20);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2017 };
        await DoTest(sut, options);

        await sut.Received(20).ExecuteAsync(Arg.Is<PuzzleKey>(k => k.Year == 2017), options);
    }
    [Fact]
    public async Task NoYearDay_DuringAdvent_RunsPuzzleForThatDayInCurrentYear()
    {
        (var year, var month, var day) = (2017, 12, 20);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { day = 15 };
        await DoTest(sut, options);

        await sut.Received(1).ExecuteAsync(Arg.Is<PuzzleKey>(k => k.Year == 2017), options);
    }
    [Fact]
    public async Task NoYearDay_OutsideAdvent_InDecember_Throws()
    {
        (var year, var month, var day) = (2017, 12, 26);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { day = 15 };

        await Assert.ThrowsAsync<ArgumentException>(() => DoTest(sut, options));
    }
    [Fact]
    public async Task NoYearDay_OutsideAdvent_Throws()
    {
        (var year, var month, var day) = (2017, 1, 1);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { day = 15 };

        await Assert.ThrowsAsync<ArgumentException>(() => DoTest(sut, options));
    }

    [Fact]
    public async Task YearDay_OutsideAdvent_RunsSinglePuzzle()
    {
        (var year, var month, var day) = (2017, 1, 1);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2016, day = 23 };
        await DoTest(sut, options);

        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2016, 23)), options);
    }
    [Fact]
    public async Task YearDay_OutsideAdventInDecember_RunsSinglePuzzle()
    {
        (var year, var month, var day) = (2016, 12, 26);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2016, day = 23 };
        await DoTest(sut, options);

        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2016, 23)), options);
    }
    [Fact]
    public async Task YearDay_DuringAdvent_RunsPuzzleForCurrentDay()
    {
        (var year, var month, var day) = (2017, 12, 20);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2017, day = 19 };
        await DoTest(sut, options);

        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2017, 19)), options);
    }
    [Fact]
    public async Task YearDay_DuringAdvent_FuturePuzzle_Throws()
    {
        (var year, var month, var day) = (2017, 12, 20);
        var sut = CreateSystemUnderTest(year, month, day);

        var options = new AoCSettings { year = 2017, day = 23 };

        
        await Assert.ThrowsAsync<InvalidPuzzleException>(async () => await DoTest(sut, options));
    }
}