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
            args.Concat(["--loglevel=Trace", "--debug"]).ToArray());
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
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "aoc.cs")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "sample.txt")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "input.txt")));
        }

        [Fact]
        public async Task InitTwice()
        {
            await Do("init", $"{Year}", $"{Day}");
            var result = await Do("init", $"{Year}", $"{Day}", "--force");
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "aoc.cs")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "sample.txt")));
            Assert.True(fileSystem.FileExists(Path.Combine($"Year{Year}", $"Day{Day:00}", "input.txt")));
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
            await Assert.ThrowsAnyAsync<AoCException>(() => Do("init", $"{Year}", $"{Day}"));
        }


        [Fact]
        public async Task Sync()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Do("sync", $"{Year}", $"{Day}"));
        }

        [Fact]
        public async Task Run()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Do("run", $"{Year}", $"{Day}"));
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
            await Assert.ThrowsAnyAsync<AoCException>(() => Do("post", "answer1", $"{Year}", $"{Day}"));
        }

        [Fact]
        public async Task Verify()
        {
            await Assert.ThrowsAnyAsync<AoCException>(() => Do("verify", $"{Year}", $"{Day}"));
        }
    }

}
