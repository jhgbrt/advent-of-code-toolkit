using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using NodaTime;

using RichardSzalay.MockHttp;

using System.Net;

using Xunit.Abstractions;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class IntegrationTests(ITestOutputHelper output, DateTime Now, (int year, int day) puzzle)
{

    private readonly IAssemblyResolver resolver = Mocks.AssemblyResolver();
    private readonly IInputOutputService io = Mocks.InputOutput(output);
    private readonly IClock clock = Mocks.Clock(Now);
    private readonly IAoCDbContext database = Mocks.DbContext();
    internal IFileSystem fileSystem = Mocks.FileSystem();
    protected int Year => puzzle.year;
    protected int Day => puzzle.day;

    protected async Task<int> Do(
        IEnumerable<string> args,
        IEnumerable<(HttpMethod method, string url, HttpStatusCode responsecode, string responseContent)> http
        )
    {
        MockHttpMessageHandler handler = Mocks.HttpMessageHandler("https://adventofcode.com", http);
        Environment.SetEnvironmentVariable("AOC_SESSION", "somecookie");
        var returnValue = await AoC.RunAsync(
            resolver,
            io,
            clock,
            database,
            handler,
            fileSystem,
            args.Concat(["--loglevel=Trace", "--debug"]).ToArray());

        handler.VerifyNoOutstandingRequest();
        handler.VerifyNoOutstandingExpectation();

        return returnValue;
    }

    IEnumerable<string> Args(string command, int? year, int? day)
    {
        yield return command;
        if (year.HasValue) yield return $"{year}";
        if (day.HasValue) yield return $"{day}";
    }

    protected Task<int> Help() => Do(["--help"], []);
    protected Task<int> Init(int? year = null, int? day = null, bool force = false)
    {
        if (!year.HasValue) year = Year;
        if (!day.HasValue) day = Day;

        IEnumerable<(HttpMethod, string, HttpStatusCode, string)> http = [
              (HttpMethod.Get, "2015/day/4/input", HttpStatusCode.OK, string.Empty)
            , (HttpMethod.Get, $"{year}/day/{day}", HttpStatusCode.OK, TestContent.HtmlContentNoAnswers)
            , (HttpMethod.Get, $"{year}/day/{day}/input", HttpStatusCode.OK, "input")
            ];

        IEnumerable<string> args = Args("init", year, day);
        if (force) args = args.Append("--force");

        return Do(args, http);
    }

    protected Task<int> Sync(int? year = null, int? day = null)
    {
        if (!year.HasValue) year = Year;
        if (!day.HasValue) day = Day;

        IEnumerable<(HttpMethod, string, HttpStatusCode, string)> http = [
              (HttpMethod.Get, "2015/day/4/input", HttpStatusCode.OK, string.Empty)
            , (HttpMethod.Get, $"{year}/day/{day}", HttpStatusCode.OK, TestContent.HtmlContentNoAnswers)
            , (HttpMethod.Get, $"{year}/day/{day}/input", HttpStatusCode.OK, "input")
            ];
        return Do(Args("sync", year, day), http);
    }

    protected Task<int> Run(int? year = null, int? day = null)
    {
        return Do(Args("run", year, day), []);
    }

    protected Task<int> Stats()
    {
        IEnumerable<(HttpMethod, string, HttpStatusCode, string)> http = [
              (HttpMethod.Get, "2015/day/4/input", HttpStatusCode.OK, string.Empty)
            , (HttpMethod.Get, $"settings", HttpStatusCode.OK, TestContent.Settings)
            , (HttpMethod.Get, $"2015/leaderboard/private/view/123.json", HttpStatusCode.OK, TestContent.Leaderboard)
            , (HttpMethod.Get, $"2016/leaderboard/private/view/123.json", HttpStatusCode.OK, TestContent.Leaderboard)
            , (HttpMethod.Get, $"2017/leaderboard/private/view/123.json", HttpStatusCode.OK, TestContent.Leaderboard)

            ];
        return Do(["stats"], http);
    }

    protected Task<int> Post(string value, int? year = null, int? day = null)
    {
        IEnumerable<string> args = ["post", value];
        if (year.HasValue) args = args.Append($"{year}");
        if (day.HasValue) args = args.Append($"{day}");

        if (!year.HasValue) year = Year;
        if (!day.HasValue) day = Day;

        IEnumerable<(HttpMethod, string, HttpStatusCode, string)> http = [
              (HttpMethod.Get, "2015/day/4/input", HttpStatusCode.OK, string.Empty)
            , (HttpMethod.Post, $"{year}/day/{day}/answer", HttpStatusCode.OK, TestContent.HtmlContentCorrectAnswerResponse)
            , (HttpMethod.Get, $"settings", HttpStatusCode.OK, TestContent.Settings)
            , (HttpMethod.Get, $"{year}/leaderboard/private/view/123.json", HttpStatusCode.OK, TestContent.Leaderboard)
            ];

        return Do(args, http);
    }

    protected Task<int> Verify(int? year = null, int? day = null)
    {
        return Do(Args("verify", year, day), []);
    }

    public class DuringAdvent_OnDayOfPuzzle(ITestOutputHelper output) 
        : IntegrationTests(output, new(2017, 12, 3), (2017, 3))
    {
        [Fact]
        public async Task TestHelp()
        {
            var result = await Help();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestInit()
        {
            var result = await Init();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestInitTwice()
        {
            await Init();
            var result = await Init(force: true);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestSync()
        {
            await Init();
            var result = await Sync();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestRun()
        {
            await Init();
            var result = await Sync();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestStats()
        {
            var result = await Stats();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestPost()
        {
            await Init();
            var result = await Post("123", Year, Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestVerify()
        {
            await Init();
            await Run();
            await Post("answer1");
            await Post("answer2");
            var result = await Verify();
            Assert.Equal(0, result);
        }
    }

    public class DuringAdvent_AfterDayOfPuzzle(ITestOutputHelper output) : IntegrationTests(output, new(2017, 12, 5), (2017, 3))
    {
        [Fact]
        public async Task TestHelp()
        {
            var result = await Help();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestInitTwice()
        {
            await Init(Year, Day, true);
            var result = await Init(day: Day, force: true);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestSync()
        {
            await Init(Year, Day);
            var result = await Sync(day: Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestRun()
        {
            await Init(Year, Day);
            var result = await Run(day: Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestStats()
        {
            var result = await Stats();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestPost()
        {
            await Init(Year, Day);
            var result = await Post("answer1", Year, Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestVerify()
        {
            await Init(Year, Day);
            await Run(Year, Day);
            await Post("answer1", Year, Day);
            await Post("answer2", Year, Day);
            var result = await Verify(Year, Day);
            Assert.Equal(0, result);
        }
    }

    public class AfterAdvent(ITestOutputHelper output) : IntegrationTests(output, new(2017, 12, 27), (2017, 3))
    {
     
        [Fact]
        public async Task TestInit()
        {
            var result = await Init(Year, Day);
            Assert.Equal(0, result);
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "aoc.cs")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "sample.txt")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "input.txt")));
        }

        [Fact]
        public async Task TestInitTwice()
        {
            await Init(Year, Day);
            var result = await Init(Year, Day, true);
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "aoc.cs")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "sample.txt")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "input.txt")));
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestSync()
        {
            await Init(Year, Day);
            var result = await Sync(day: Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestRun()
        {
            await Init(Year, Day);
            var result = await Run(day: Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestStats()
        {
            var result = await Stats();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestPost()
        {
            await Init(Year, Day);
            await Post("answer1", Year, Day);
            var result = await Post("answer2", Year, Day);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestVerify()
        {
            await Init(Year, Day);
            await Run(Year, Day);
            await Post("answer1", Year, Day);
            await Post("answer2", Year, Day);
            var result = await Verify(Year, Day);
            Assert.Equal(0, result);
        }
    }

    public class LockedPuzzle(ITestOutputHelper output) : IntegrationTests(output, new(2017, 12, 1), (2019, 3))
    {
        [Fact]
        public async Task TestHelp()
        {
            var result = await Help();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestInit()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Init(Year, Day));
        }


        [Fact]
        public async Task TestSync()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Sync(Year, Day));
        }

        [Fact]
        public async Task TestRun()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Run(Year, Day));
        }

        [Fact]
        public async Task TestStats()
        {
            var result = await Stats();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task TestPost()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Post("answer1", Year, Day));
        }

        [Fact]
        public async Task TestVerify()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Verify(Year, Day));
        }
    }

}
