using Net.Code.AdventOfCode.Toolkit.Commands;
using Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using NSubstitute;

using Spectre.Console;
using Spectre.Console.Rendering;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Net.Code.AdventOfCode.Toolkit.IntegrationTests
{
    class TestOutputService : IInputOutputService
    {
        ITestOutputHelper output;

        public TestOutputService(ITestOutputHelper output) => this.output = output;

        public void MarkupLine(string markup) => output.WriteLine(markup);

        public T Prompt<T>(IPrompt<T> prompt) => default!;

        public void Write(IRenderable renderable) => output.WriteLine(renderable.ToString());

        public void WriteLine(string message) => output.WriteLine(message);
    }

    public class IntegrationTests
    {

        private IAssemblyResolver resolver;
        private TestOutputService io;
        private IClock clock;
        protected int Year => puzzle.year;
        protected int Day => puzzle.day;
        private readonly (int year, int day) puzzle;
        private readonly DateTime Now;

        public IClock Clock()
        {
            var localdate = new LocalDateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0);
            var instant = localdate.InZoneLeniently(DateTimeZoneProviders.Tzdb["EST"]).ToInstant();
            var clock = Substitute.For<IClock>();
            clock.GetCurrentInstant().Returns(instant);
            return clock;
        }

        public IntegrationTests(ITestOutputHelper output, DateTime now, (int year, int day) puzzle)
        {
            Now = now;
            this.puzzle = puzzle;
            resolver = Substitute.For<IAssemblyResolver>();
            var assembly = Assembly.GetExecutingAssembly();
            resolver.GetEntryAssembly().Returns(assembly);
            io = new TestOutputService(output);
            output.WriteLine(Environment.CurrentDirectory);

            if (Directory.Exists(".cache"))
            {
                Directory.Delete(".cache", true);
            }
            if (Directory.Exists("Year2017"))
            {
                Directory.Delete("Year2017", true);
            }
            clock = Clock();
        }
        protected async Task<int> Do(params string[] args)
        {
            return await AoC.RunAsync(resolver, io, clock, args.Concat(new[] { "--debug", "--loglevel=Trace" }).ToArray());
        }
        public class DuringAdvent_OnDayOfPuzzle : IntegrationTests
        {
            public DuringAdvent_OnDayOfPuzzle(ITestOutputHelper output) : base(output, new(2017, 12, 3), (2017, 3)) { }

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
                var result = await Do("post", "123");
                Assert.Equal(1, result); // already completed
            }

            [Fact]
            public async Task Verify()
            {
                await Do("init");
                await Do("run");
                var result = await Do("verify");
                Assert.Equal(0, result);
            }
        }
        public class DuringAdvent_AfterDayOfPuzzle : IntegrationTests
        {
            public DuringAdvent_AfterDayOfPuzzle(ITestOutputHelper output) : base(output, new(2017, 12, 3), (2017, 1)) { }

            [Fact]
            public async Task Help()
            {
                var result = await Do("--help");
                Assert.Equal(0, result);
            }

            [Fact]
            public async Task InitTwice()
            {
                await Do("init", $"{Day}");
                var result = await Do("init", $"{Day}", "--force");
                Assert.Equal(0, result);
            }

            [Fact]
            public async Task Sync()
            {
                await Do("init", $"{Day}");
                var result = await Do("sync", $"{Day}");
                Assert.Equal(0, result);
            }

            [Fact]
            public async Task Run()
            {
                await Do("init", $"{Day}");
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
                await Do("init", $"{Day}");
                var result = await Do("post", "123", $"{Year}", $"{Day}");
                Assert.Equal(1, result); // already completed
            }

            [Fact]
            public async Task Verify()
            {
                await Do("init", $"{Day}");
                await Do("run", $"{Day}");
                var result = await Do("verify", $"{Day}");
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
                Assert.True(File.Exists(Path.Combine($"Year{Year}", $"Day{Day:00}", "aoc.cs")));
                Assert.True(File.Exists(Path.Combine($"Year{Year}", $"Day{Day:00}", "sample.txt")));
                Assert.True(File.Exists(Path.Combine($"Year{Year}", $"Day{Day:00}", "input.txt")));
            }

            [Fact]
            public async Task InitTwice()
            {
                await Do("init", $"{Year}", $"{Day}");
                var result = await Do("init", $"{Year}", $"{Day}", "--force");
                Assert.True(File.Exists(Path.Combine($"Year{Year}", $"Day{Day:00}", "aoc.cs")));
                Assert.True(File.Exists(Path.Combine($"Year{Year}", $"Day{Day:00}", "sample.txt")));
                Assert.True(File.Exists(Path.Combine($"Year{Year}", $"Day{Day:00}", "input.txt")));
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
                Assert.True(File.Exists(Path.Combine(".cache", $"{Year}", $"{Day:00}", "result.json")));
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
                var result = await Do("post", "123", $"{Year}", $"{Day}", "--loglevel=Trace");
                Assert.Equal(1, result); // already completed
            }

            [Fact]
            public async Task Verify()
            {
                await Do("init", $"{Year}", $"{Day}");
                await Do("run", $"{Year}", $"{Day}");
                var result = await Do("verify", $"{Year}", $"{Day}");
                Assert.Equal(0, result);
            }
        }
    }

}
namespace Year2017
{
    namespace Day01
    {
        public class AoC201701
        {
            public object Part1() => 1034;
            public object Part2() => 1356;
        }
    }
    namespace Day03
    {
        public class AoC201703
        {
            public object Part1() => 438;
            public object Part2() => 266330;
        }
    }
}
