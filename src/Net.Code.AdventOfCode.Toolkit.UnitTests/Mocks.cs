using Castle.Core.Logging;

using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Net.Code.AdventOfCode.Toolkit.Web;

using NodaTime;

using NSubstitute;

using RichardSzalay.MockHttp;

using Spectre.Console;
using Spectre.Console.Rendering;

using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

using Xunit.Abstractions;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    static class TestContent
    {
        public static string HtmlContentNoAnswers => content_0_answers;
        public static string HtmlContentOneAnswer => content_1_answers;
        public static string HtmlContentTwoAnswers => content_2_answers;
        public static string HtmlContentCorrectAnswerResponse => correct;
        public static string HtmlContentWrongAnswerResponse => wrong;
        public static string Settings => settings;
        public static string Leaderboard => leaderboard;
        const string content_0_answers = """
            <!DOCTYPE html>
            <html lang="en-us">
            <body>
            <p>content for puzzle 2017 day 3 - part 1</p>
            </body>
            </html>
            """;
        const string content_1_answers = """
            <!DOCTYPE html>
            <html lang="en-us">
            <body>
            <p>content for puzzle 2017 day 3 - part 1</p>
            <p>Your puzzle answer was <code>answer1</code>.</p>
            <p>content for puzzle 2017 day 3 - part 2</p>
            </body>
            </html>
            """;
        const string content_2_answers = """
            <!DOCTYPE html>
            <html lang="en-us">
            <body>
            <p>content for puzzle 2017 day 3 - part 1</p>
            <p>Your puzzle answer was <code>answer1</code>.</p>
            <p>content for puzzle 2017 day 3 - part 2</p>
            <p>Your puzzle answer was <code>answer2</code>.</p>
            </body>
            </html>
            """;
        const string correct = """
            <!DOCTYPE html>
            <html lang="en-us">
            <body>
            <article>That's the right answer!</article>
            </body>
            """;

        const string wrong = """
            <!DOCTYPE html>
            <html lang="en-us">
            <body>
            <article>That's NOT the right answer!</article>
            </body>
            """;
        const string settings = """
            <!DOCTYPE html>
            <html lang="en-us">
            <head>
            <meta charset="utf-8"/>
            <title>Settings - Advent of Code 2022</title>
            </head>
            <body>
            <main>
            <article>
            <p>What would you like to be called?</p>
            <div><label><input type="radio" name="display_name" onchange="anon(true)" value="anonymous"/><span>(anonymous user #123)</span></label></div>
            <div><label><input type="radio" name="display_name" onchange="anon(false)" value="0" checked="checked"/><span><img src="https://avatars.githubusercontent.com/u/12345" height="20"/>name</span></label></div>
            <p><label><input type="checkbox" id="display_url" name="display_url" value="1" checked="checked"/><span>Link to https://github.com/REDACTED</span></label></p>
            <p>Sponsor join code: <input type="text" name="sponsor_join" oninput="document.getElementById('sponsor-privboard-warning').classList[/-/.exec(this.value)?'add':'remove']('warning-active')"/> <span class="quiet">(Leave blank unless you are an employee (or similar) of a sponsor. </span><span id="sponsor-privboard-warning" class="quiet warning">Not for <a href="/2022/leaderboard/private">private leaderboard</a> codes.</span><span class="quiet">)</span></p><p style="margin-bottom:3em;"><input type="submit" value="[Save]"/></p></form>
            <p style="width:45em;">Advanced actions:</p>
            <ul style="width:45em;">
            <li>Provide <span class="hidden-until-hover"><code>ownerproof-REDACTED</code></span> if you are asked to prove you own this account by an Advent of Code administrator. Don't post this code in a public place.</li>
            </ul>
            </article>
            </main>
            </body>
            </html>
            """;

        const string leaderboard = """
            {
                "owner_id": 123,
                "event": "2015",
                "members": {
                    "123": {
                        "stars": 0,
                        "local_score": 0,
                        "global_score": 0,
                        "completion_day_level": {},
                        "id": 123,
                        "name": "member1",
                        "last_star_ts": 0
                    },
                    "456": {
                        "stars": 0,
                        "local_score": 0,
                        "id": 456,
                        "name": "member2",
                        "last_star_ts": 0,
                        "global_score": 0,
                        "completion_day_level": {}
                    }
                }
            }
            """;

    }

    static class Mocks
    {
        public static IInputOutputService InputOutput(ITestOutputHelper output) => new TestOutputService(output);
        public static IAoCDbContext DbContext() => new TestDbContext();
        public static IFileSystem FileSystem() => new TestFileSystem();
        public static MockHttpMessageHandler HttpMessageHandler(
            string baseAddress,
            params IEnumerable<(HttpMethod method, string path, HttpStatusCode responseCode, string responseContent)> items)
        {
            var handler = new MockHttpMessageHandler();
            foreach (var item in items)
            {
                handler
                    .Expect(item.method, $"{baseAddress}/{item.path}")
                    .Respond(req => new HttpResponseMessage(item.responseCode) { Content = new StringContent(item.responseContent) });
            }
            return handler;
        }

        public static IAssemblyResolver AssemblyResolver()
        {
            var resolver = Substitute.For<IAssemblyResolver>();
            var assembly = Assembly.GetExecutingAssembly();
            resolver.GetEntryAssembly().Returns(assembly);
            return resolver;
        }
        public static IClock Clock(DateTime Now)
        {
            var localdate = new LocalDateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0);
            var instant = localdate.InZoneLeniently(DateTimeZoneProviders.Tzdb["EST"]).ToInstant();
            var clock = Substitute.For<IClock>();
            clock.GetCurrentInstant().Returns(instant);
            return clock;
        }

        class TestOutputService : IInputOutputService
        {
            ITestOutputHelper output;

            public TestOutputService(ITestOutputHelper output) => this.output = output;

            public void MarkupLine(string markup) => output.WriteLine(markup);

            public T Prompt<T>(IPrompt<T> prompt) => default!;

            public void Write(IRenderable renderable) => output.WriteLine(renderable.ToString());

            public void WriteLine(string message) => output.WriteLine(message);
        }
        class TestDbContext : IAoCDbContext
        {
            Dictionary<PuzzleKey, Puzzle> puzzles = new[]
            {
                Puzzle.Create(new(2017,1), "input", new("answer1", "answer2")),
                Puzzle.Create(new(2017,2), "input", new("answer1", "")),
                Puzzle.Create(new(2017,3), "input", new("", "")),
            }.ToDictionary(p => p.Key);

            Dictionary<PuzzleKey, DayResult> results = new[]
            {
            new DayResult(new(2017, 1), new Result(ResultStatus.Ok, "answer1", TimeSpan.FromMilliseconds(5)), new Result(ResultStatus.Ok, "answer2", TimeSpan.FromMilliseconds(5))),
            new DayResult(new(2017, 2), new Result(ResultStatus.Ok, "answer1", TimeSpan.FromMilliseconds(5)), Result.Empty),
            DayResult.NotImplemented(new(2017, 3))
        }.ToDictionary(r => r.Key);

            public IQueryable<Puzzle> Puzzles => puzzles.Values.AsQueryable();

            public IQueryable<DayResult> Results => results.Values.AsQueryable();

            public void AddPuzzle(Puzzle puzzle)
            {
                puzzles.Add(puzzle.Key, puzzle);
            }

            public void AddResult(DayResult result)
            {
                results.Add(result.Key, result);
            }

            public ValueTask<Puzzle?> GetPuzzle(PuzzleKey key)
            {
                puzzles.TryGetValue(key, out var puzzle);
                return ValueTask.FromResult(puzzle);
            }

            public ValueTask<DayResult?> GetResult(PuzzleKey key)
            {
                results.TryGetValue(key, out var result);
                return ValueTask.FromResult(result);
            }

            public void Migrate()
            {
            }
            public void Dispose() { }

            public Task<int> SaveChangesAsync(CancellationToken token = default)
            {
                return Task.FromResult(0);
            }
        }

        class TestFileSystem : IFileSystem
        {

            public string CurrentDirectory => @"C:\aoc";
            public TestFileSystem()
            {
                var template = Path.Combine(CurrentDirectory, "Template");
                data[template] = new();
                data[template]["aoc.cs"] = """
                namespace AdventOfCode.Year_YYYY_.Day_DD_;
                public class AoC_YYYY__DD_
                {
                    static string[] input = Read.InputLines();
                    public object Part1() => "";
                    public object Part2() => "";
                }
                """;
                data[template]["aoc.csproj"] = """
                <?xml version="1.0" encoding="utf-8"?>
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net6.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                  <ItemGroup>
                    <Using Include="Microsoft.FSharp.Collections" />
                    <Using Include="System.Diagnostics" />
                    <Using Include="System.Reflection" />
                    <Using Include="System.Text" />
                    <Using Include="System.Text.Json" />
                    <Using Include="System.Text.RegularExpressions" />
                    <Using Include="System.Collections.Immutable" />
                    <Using Include="System.Linq.Enumerable" Static="true" />
                    <Using Include="System.Math" Static="true" />
                  </ItemGroup>
                  <ItemGroup>
                    <PackageReference Include="FSharp.Core" Version="6.0.1" />
                  </ItemGroup>
                </Project>
                """;
            }

            Dictionary<string, Dictionary<string, string>> data = new();

            private string FullyQualifiedPath(string path)
            {
                if (Path.IsPathFullyQualified(path)) return path;
                return Path.Combine(CurrentDirectory, path);
            }

            public void CreateDirectoryIfNotExists(string path, FileAttributes? attributes)
            {
                path = FullyQualifiedPath(path);
                if (data.ContainsKey(path)) return;
                data[path] = new();
            }

            public Task<string> ReadAllTextAsync(string path)
            {
                path = FullyQualifiedPath(path);
                var dir = Path.GetDirectoryName(path)!;
                var file = Path.GetFileName(path)!;
                var content = data[dir][file];
                return Task.FromResult(content);
            }
            public Task WriteAllTextAsync(string path, string content)
            {
                path = FullyQualifiedPath(path);
                var dir = Path.GetDirectoryName(path)!;
                var file = Path.GetFileName(path)!;
                data[dir][file] = content;
                return Task.FromResult(0);
            }
            public bool FileExists(string path)
            {
                path = FullyQualifiedPath(path);
                var dir = Path.GetDirectoryName(path)!;
                var file = Path.GetFileName(path)!;
                return data.TryGetValue(dir, out var d) && d.ContainsKey(file);
            }
            public bool DirectoryExists(string path)
            {
                path = FullyQualifiedPath(path);
                var dir = Path.GetDirectoryName(path)!;
                var exists = data.TryGetValue(path, out var d);
                return exists;
            }

            public void DeleteFile(string path)
            {
                path = FullyQualifiedPath(path);
                var dir = Path.GetDirectoryName(path)!;
                var file = Path.GetFileName(path)!;
                data[dir].Remove(file);
            }
        }

    }

}
