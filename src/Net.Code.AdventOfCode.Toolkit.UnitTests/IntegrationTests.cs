using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using NodaTime;

using Xunit.Abstractions;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class IntegrationTests
{ 

    private IAssemblyResolver resolver;
    private IInputOutputService io;
    private IClock clock;
    private IAoCDbContext database;
    private IHttpClientWrapper client;
    internal IFileSystem fileSystem;
    protected int Year => puzzle.year;
    protected int Day => puzzle.day;
    private readonly (int year, int day) puzzle;
    private readonly DateTime Now;
    protected ITestOutputHelper output;


    public IntegrationTests(ITestOutputHelper output, DateTime now, (int year, int day) puzzle)
    {
        this.output = output;
        Now = now;
        this.puzzle = puzzle;
        resolver = Mocks.AssemblyResolver();
        io = Mocks.InputOutput(output);
        clock = Mocks.Clock(Now);
        database = Mocks.DbContext();
        client = Mocks.HttpClientWrapper();
        fileSystem = Mocks.FileSystem();
    }

    protected async Task<int> Do(params string[] args)
    {
        Environment.SetEnvironmentVariable("AOC_SESSION", "somecookie");
        return await AoC.RunAsync(
            resolver, 
            io, 
            clock, 
            database,
            client,
            fileSystem,
            args.Concat(new[] { "--loglevel=Trace" }).ToArray());
    }

    public class DuringAdvent_OnDayOfPuzzle : IntegrationTests
    {
        public DuringAdvent_OnDayOfPuzzle(ITestOutputHelper output) : base(output, new(2017, 12, 3), (2017, 3))
        {
        }

        [Fact]
        public async Task Help()
        {
            var result = await Do("--help");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task InitTwice()
        {
            await Do("init");
            var result = await Do("init", "--force");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Init()
        {
            var result = await Do("init", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("aoc.cs"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("sample.txt"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("input.txt"))));
        }

        [Fact]
        public async Task Sync()
        {
            await Do("init");
            var result = await Do("sync");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Run()
        {
            await Do("init");
            var result = await Do("run");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Stats()
        {
            var result = await Do("stats");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Post()
        {
            await Do("init");
            var result = await Do("post", "123", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Verify()
        {
            await Do("init");
            await Do("run");
            await Do("post", "answer1");
            await Do("post", "answer2");
            var result = await Do("verify");
            Assert.Equal(0, result);
        }
    }

    public class DuringAdvent_AfterDayOfPuzzle : IntegrationTests
    {
        public DuringAdvent_AfterDayOfPuzzle(ITestOutputHelper output) : base(output, new(2017, 12, 5), (2017, 3)) { }

        [Fact]
        public async Task Help()
        {
            var result = await Do("--help");
            Assert.Equal(0, result);
        }
        [Fact]
        public async Task Init()
        {
            var result = await Do("init", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("aoc.cs"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("sample.txt"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("input.txt"))));
        }

        [Fact]
        public async Task InitTwice()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("init", $"{Day}", "--force");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Sync()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("sync", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Run()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("run", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Stats()
        {
            var result = await Do("stats");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Post()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("post", "answer1", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Verify()
        {
            await Do("init", $"{Year}", $"{Day}");
            await Do("run", $"{Year}", $"{Day}");
            await Do("post", "answer1", $"{Year}", $"{Day}");
            await Do("post", "answer2", $"{Year}", $"{Day}");
            var result = await Do("verify", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }
    }

    public class AfterAdvent : IntegrationTests
    {
        public AfterAdvent(ITestOutputHelper output) : base(output, new(2017, 12, 27), (2017, 3)) { }

        [Fact]
        public async Task Help()
        {
            var result = await Do("--help");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Init()
        {
            var result = await Do("init", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("aoc.cs"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("sample.txt"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("input.txt"))));
        }

        [Fact]
        public async Task InitTwice()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("init", $"{Year}", $"{Day}", "--force");
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("aoc.cs"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("sample.txt"))));
            Assert.True(fileSystem.Exists((FileInfo)new(new(@$"c:\aoc\Year{Year}\Day{Day:00}"), new("input.txt"))));
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Sync()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("sync", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Run()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("run", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Stats()
        {
            var result = await Do("stats");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Post()
        {
            await Do("init", $"{Year}", $"{Day}");
            await Do("post", "answer1", $"{Year}", $"{Day}");
            var result = await Do("post", "answer2", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Verify()
        {
            await Do("init", $"{Year}", $"{Day}");
            await Do("run", $"{Year}", $"{Day}");
            await Do("post", "answer1", $"{Year}", $"{Day}");
            await Do("post", "answer2", $"{Year}", $"{Day}");
            var result = await Do("verify", $"{Year}", $"{Day}");
            Assert.Equal(0, result);
        }
    }

    public class LockedPuzzle : IntegrationTests
    {
        public LockedPuzzle(ITestOutputHelper output) : base(output, new(2017, 12, 1), (2019, 3)) { }

        [Fact]
        public async Task Help()
        {
            var result = await Do("--help");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Init()
        {
            var result = await Do("init", $"{Year}", $"{Day}");
            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task InitTwice()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("init", $"{Year}", $"{Day}", "--force");
            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task Sync()
        {
            var result = await Do("sync", $"{Year}", $"{Day}");
            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task Run()
        {
            var result = await Do("run", $"{Year}", $"{Day}");
            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task Stats()
        {
            var result = await Do("stats");
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Post()
        {
            var result = await Do("post", "answer1", $"{Year}", $"{Day}");
            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task Verify()
        {
            await Do("init", $"{Year}", $"{Day}");
            await Do("run", $"{Year}", $"{Day}");
            await Do("post", "answer1", $"{Year}", $"{Day}");
            await Do("post", "answer2", $"{Year}", $"{Day}");
            var result = await Do("verify", $"{Year}", $"{Day}");
            Assert.NotEqual(0, result);
        }
    }

}
