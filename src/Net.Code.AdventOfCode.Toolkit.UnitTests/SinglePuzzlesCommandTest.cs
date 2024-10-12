using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using NSubstitute;
using NSubstitute.Extensions;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class SinglePuzzlesCommandTest
{
    private static SinglePuzzleCommand<AoCSettings> CreateSystemUnderTest(int year, int month, int day)
    {
        var clock = TestClock.Create(year, month, day, 0, 0, 0);
        var logic = new AoCLogic(clock);
        var sut = Substitute.ForPartsOf<SinglePuzzleCommand<AoCSettings>>(logic);
        return sut;
    }
    private async Task DoTest(SinglePuzzleCommand<AoCSettings> sut, AoCSettings options)
    {
        var context = new CommandContext([], Substitute.For<IRemainingArguments>(), "name", default);
        await sut.Configure().ExecuteAsync(Arg.Any<PuzzleKey>(), options);
        await sut.ExecuteAsync(context, options);
    }

    [Fact]
    public async Task NoYearNoDay_BeforeAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 1, 1);
        var options = new AoCSettings();
        await Assert.ThrowsAnyAsync<AoCException>(() => DoTest(sut, options));
    }

    [Fact]
    public async Task NoYearNoDay_AfterAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 12, 26);
        var options = new AoCSettings();
        await Assert.ThrowsAnyAsync<AoCException>(() => DoTest(sut, options));
    }

    [Fact]
    public async Task NoYearNoDay_DuringAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 12, 20);
        var options = new AoCSettings();
        await DoTest(sut, options);
        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2017, 20)), options);
    }

    [Fact]
    public async Task YearNoDay_BeforeAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 1, 1);
        var options = new AoCSettings { year = 2017 };
        await Assert.ThrowsAnyAsync<AoCException>(() => DoTest(sut, options));
    }

    [Fact]
    public async Task YearNoDay_AfterAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 12, 27);
        var options = new AoCSettings { year = 2017 };
        await Assert.ThrowsAnyAsync<AoCException>(() => DoTest(sut, options));
    }

    [Fact]
    public async Task YearNoDay_DuringAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 12, 20);
        var options = new AoCSettings { year = 2017 };
        await DoTest(sut, options);
        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2017, 20)), options);
    }

    [Fact]
    public async Task YearDay_Past_RunsSinglePuzzle()
    {
        var sut = CreateSystemUnderTest(2017, 1, 1);
        var options = new AoCSettings { year = 2016, day = 23 };
        await DoTest(sut, options);
        await sut.Received(1).ExecuteAsync(Arg.Is(new PuzzleKey(2016, 23)), options);
    }

    [Fact]
    public async Task YearDay_Future_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 1, 1);
        var options = new AoCSettings { year = 2018, day = 23 };
        await Assert.ThrowsAnyAsync<AoCException>(() => DoTest(sut, options));
    }

    [Fact]
    public async Task YearDay_Future_DuringAdvent_Throws()
    {
        var sut = CreateSystemUnderTest(2017, 12, 20);
        var options = new AoCSettings { year = 2017, day = 23 };
        await Assert.ThrowsAnyAsync<AoCException>(() => DoTest(sut, options));
    }

}
