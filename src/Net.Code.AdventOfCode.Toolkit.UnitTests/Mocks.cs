using Castle.Core.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NodaTime;

using NSubstitute;

using Spectre.Console;
using Spectre.Console.Rendering;

using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

using Xunit.Abstractions;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    static class Mocks
    {
        public static IInputOutputService InputOutput(ITestOutputHelper output) => new TestOutputService(output);
        public static IHttpClientWrapper HttpClientWrapper() => new TestHttpWrapper();
        public static IAoCDbContext DbContext() => new TestDbContext();
        public static IFileSystem FileSystem() => new TestFileSystem();
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
        class TestHttpWrapper : IHttpClientWrapper
        {
            public void Dispose()
            {
            }

            Dictionary<string, string> values = new()
            {
                ["2015/day/4/input"] = "input",
                ["2017/day/3"] = content_0_answers,
                ["2017/day/3/input"] = "input",
                ["2017/day/5"] = content_0_answers,
                ["2017/day/5/input"] = "input",
                ["settings"] = settings,
                ["2015/leaderboard/private/view/123.json"] = leaderboard,
                ["2016/leaderboard/private/view/123.json"] = leaderboard,
                ["2017/leaderboard/private/view/123.json"] = leaderboard,
                ["2018/leaderboard/private/view/123.json"] = leaderboard,
                ["2019/leaderboard/private/view/123.json"] = leaderboard


            };

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

            public Task<(HttpStatusCode status, string content)> GetAsync(string path)
            {
                return Task.FromResult((HttpStatusCode.OK, values[path]));
            }

            public async Task<(HttpStatusCode status, string content)> PostAsync(string path, HttpContent body)
            {
                var contentpath = path[0..^7];
                var content = values[contentpath];
                if (body is not FormUrlEncodedContent f)
                {
                    throw new Exception("expected form url encoded content");
                }

                var bodyregex = new Regex(@"level=(?<level>\d+)&answer=(?<answer>.*)", RegexOptions.Compiled);
                var match = bodyregex.Match(await f.ReadAsStringAsync());

                var part = match.Groups["level"].Value;
                var answer = match.Groups["answer"].Value;

                (values[contentpath], var status, var result) = (part, answer, content) switch
                {
                    ("1", "answer1", content_0_answers) => (content_1_answers, HttpStatusCode.OK, correct),
                    ("2", "answer2", content_1_answers) => (content_2_answers, HttpStatusCode.OK, correct),
                    ("1" or "2", _, content_0_answers or content_1_answers) => (content, HttpStatusCode.OK, wrong),
                    _ => (content, HttpStatusCode.OK, "something is wrong")
                };
                return (status, result);
            }
        }
        class TestFileSystem : IFileSystem
        {

            public DirectoryInfo CurrentDirectory => new(@"c:\aoc");
            public TestFileSystem()
            {
                var template = CurrentDirectory.GetDirectory("Template");
                data[template] = new();
                data[template][new("aoc.cs")] = """
                namespace AdventOfCode.Year_YYYY_.Day_DD_;
                public class AoC_YYYY__DD_
                {
                    static string[] input = Read.InputLines();
                    public object Part1() => "";
                    public object Part2() => "";
                }
                """;
                data[template][new("aoc.csproj")] = """
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

            Dictionary<DirectoryInfo, Dictionary<FileName, string>> data = new();

            private string FullyQualifiedPath(string path)
            {
                if (System.IO.Path.IsPathFullyQualified(path)) return path;
                return System.IO.Path.Combine(CurrentDirectory.FullName, path);
            }

            public void Create(DirectoryInfo path)
            {
                if (data.ContainsKey(path)) return;
                data[path] = new();
            }

            public Task<string> Read(FileInfo path)
            {
                var dir = path.Directory;
                var file = path.Name;
                var content = data[dir][file];
                return Task.FromResult(content);
            }
            public Task Write(FileInfo path, string content)
            {
                var dir = path.Directory;
                var file = path;
                data[dir][file.Name] = content;
                return Task.FromResult(0);
            }
            public bool Exists(FileInfo path)
            {
                var dir = path.Directory;
                var file = path.Name;
                return data.TryGetValue(dir, out var d) && d.ContainsKey(file);
            }
            public bool Exists(DirectoryInfo dir)
            {
                return data.TryGetValue(dir, out var _);
            }

            public void Delete(FileInfo path)
            {
                var dir = path.Directory;
                var file = path.Name;
                data[dir].Remove(file);
            }

            public void Delete(DirectoryInfo path)
            {
                data.Remove(path);
            }

            public IEnumerable<FileInfo> GetFiles(DirectoryInfo dir, string pattern)
            {
                return data[dir].Select(n => new FileInfo(dir,n.Key));
            }

            public void Copy(FileInfo source, FileInfo destination)
            {
                if (!data.TryGetValue(destination.Directory, out var d))
                {
                    d = new();
                    data[destination.Directory] = d;
                }
                d[destination.Name] = data[source.Directory][source.Name];
            }

            public void Copy(IEnumerable<FileInfo> sources, DirectoryInfo destination)
            {
                foreach (var source in sources) Copy(source, destination);
            }

            public void Copy(FileInfo source, DirectoryInfo destination)
            {
                Copy(source, new FileInfo(destination, source.Name));
            }
        }

    }

}
