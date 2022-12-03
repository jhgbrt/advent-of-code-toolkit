namespace Net.Code.AdventOfCode.Toolkit.Logic;

using HtmlAgilityPack;
using System.Net;
using NodaTime;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Net.Code.AdventOfCode.Toolkit.Core;
using Microsoft.CodeAnalysis;
using System.Linq;

class AoCClient : IDisposable, IAoCClient
{
    readonly IHttpClientWrapper client;
    private readonly ILogger<AoCClient> logger;
    private readonly ICache cache;

    public AoCClient(IHttpClientWrapper client, ILogger<AoCClient> logger, ICache cache)
    {
        this.client = client;
        this.logger = logger;
        this.cache = cache;
    }

    public async Task<(HttpStatusCode status, string content)> PostAnswerAsync(int year, int day, int part, string value)
    {
        var formValues = new Dictionary<string, string>()
        {
            ["level"] = part.ToString(),
            ["answer"] = value
        };
        var content = new FormUrlEncodedContent(formValues);
        (var status, var body) = await client.PostAsync($"{year}/day/{day}/answer", content);

        var document = new HtmlDocument();
        document.LoadHtml(body);
        var articles = document.DocumentNode.SelectNodes("//article").ToArray();

        return (status, articles.First().InnerText);
    }

    public async Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id, bool usecache = true)
    {
        (var statusCode, var content) = await GetAsync(year, null, $"leaderboard-{id}.json", $"{year}/leaderboard/private/view/{id}.json", usecache);
        if (statusCode != HttpStatusCode.OK || content.StartsWith("<"))
            return null;
        return Deserialize(year, content);
    }

    public async Task<Member?> GetMemberAsync(int year, bool usecache = true)
    {
        var id = await GetMemberId();
        var lb = await GetLeaderBoardAsync(year, id, usecache);
        if (lb is null) return null;
        return lb.Members[id];
    }

    private static LeaderBoard Deserialize(int year, string content)
    {
        var jobject = JsonDocument.Parse(content).RootElement;

        var (ownerid, members) = jobject.EnumerateObject()
            .Aggregate(
                (ownerid: -1, members: Enumerable.Empty<Member>()),
                (m, p) => p.Name switch
                {
                    "owner_id" => (GetInt32(p.Value), m.members),
                    "members" => (m.ownerid, GetMembers(p.Value)),
                    _ => m
                }
            );

        return new LeaderBoard(ownerid, year, members.ToDictionary(m => m.Id));

        int GetInt32(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetInt32(),
                JsonValueKind.String => int.Parse(value.GetString()!),
                _ => throw new InvalidOperationException("expected string or number")
            };
        }

        IEnumerable<Member> GetMembers(JsonElement element)
        {
            foreach (var item in element.EnumerateObject())
            {
                var member = item.Value;
                var result = new Member(0, string.Empty, 0, 0, 0, null, new Dictionary<int, DailyStars>());
                foreach (var property in member.EnumerateObject())
                {
                    result = property.Name switch
                    {
                        "name" => result with { Name = property.Value.GetString()! },
                        "id" => result with { Id = GetInt32(property.Value) },
                        "stars" when property.Value.ValueKind is JsonValueKind.Number => result with { TotalStars = property.Value.GetInt32() },
                        "local_score" when property.Value.ValueKind is JsonValueKind.Number => result with { LocalScore = property.Value.GetInt32() },
                        "global_score" when property.Value.ValueKind is JsonValueKind.Number => result with { GlobalScore = property.Value.GetInt32() },
                        "last_star_ts" when property.Value.ValueKind is JsonValueKind.Number => result with { LastStarTimeStamp = Instant.FromUnixTimeSeconds(property.Value.GetInt32()) },
                        "completion_day_level" => result with { Stars = GetCompletions(property).ToDictionary(x => x.Day) },
                        _ => result
                    };
                }
                yield return result;
            }
        }

        IEnumerable<DailyStars> GetCompletions(JsonProperty property)
        {
            foreach (var compl in property.Value.EnumerateObject())
            {
                var day = int.Parse(compl.Name);
                var ds = new DailyStars(day, null, null);

                foreach (var star in compl.Value.EnumerateObject())
                {
                    var instant = Instant.FromUnixTimeSeconds(star.Value.EnumerateObject().First().Value.GetInt32());
                    ds = int.Parse(star.Name) switch
                    {
                        1 => ds with { FirstStar = instant },
                        2 => ds with { SecondStar = instant },
                        _ => ds,
                    };
                }
                yield return ds;
            }

        }
    }

    private async Task<(HttpStatusCode StatusCode, string Content)> GetAsync(int? year, int? day, string name, string path, bool usecache)
    {
        string content;
        if (!cache.Exists(year, day, name) || !usecache)
        {
            (var status, content) = await client.GetAsync(path);
            if (status != HttpStatusCode.OK) return (status, content);
            await cache.WriteToCache(year, day, name, content);
        }
        else
        {
            content = await cache.ReadFromCache(year, day, name);
        }
        return (HttpStatusCode.OK, content);
    }

    public async Task<string> GetPuzzleInputAsync(int year, int day)
    {
        (var statusCode, var input) = await GetAsync(year, day, "input.txt", $"{year}/day/{day}/input", true);
        if (statusCode != HttpStatusCode.OK) return string.Empty;
        return input;
    }

    public async Task<Puzzle> GetPuzzleAsync(int year, int day, bool usecache = true)
    {
        HttpStatusCode statusCode;
        (statusCode, var html) = await GetAsync(year, day, "puzzle.html", $"{year}/day/{day}", usecache);
        if (statusCode != HttpStatusCode.OK) return Puzzle.Locked(year, day);

        var input = await GetPuzzleInputAsync(year, day);

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var articles = document.DocumentNode.SelectNodes("//article").ToArray();

        var answers = (
            from node in document.DocumentNode.SelectNodes("//p")
            where node.InnerText.StartsWith("Your puzzle answer was")
            select node.SelectSingleNode("code")
            ).ToArray();

        var answer = answers.Length switch
        {
            2 => new Answer(answers[0].InnerText, answers[1].InnerText),
            1 => new Answer(answers[0].InnerText, string.Empty),
            0 => Answer.Empty,
            _ => throw new Exception($"expected 0, 1 or 2 answers, not {answers.Length}")
        };

        var innerHtml = string.Join("", articles.Zip(answers.Select(a => a.ParentNode)).Select(n => n.First.InnerHtml + n.Second.InnerHtml));
        var innerText = string.Join("", articles.Zip(answers.Select(a => a.ParentNode)).Select(n => n.First.InnerText + n.Second.InnerText));

        return Puzzle.Unlocked(year, day, innerHtml, innerText, input, answer);
    }

    public async Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(bool usecache)
    {
        var year = DateTime.Now.Year;
        (var statusCode, var html) = await GetAsync(null, null, "leaderboard.html", $"{year}/leaderboard/private", usecache);
        if (statusCode != HttpStatusCode.OK) return Enumerable.Empty<(int, string)>();

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var link = new Regex(@"/\d+/leaderboard/private/view/(?<id>\d+)");

        var id =
            from a in document.DocumentNode.SelectNodes("//a")
            where a.InnerText == "[View]"
            let href = a.Attributes["href"].Value
            let match = link.Match(href)
            where match.Success
            let description = a.ParentNode.Name == "div" ? a.ParentNode.InnerText.Trim() : "Your own private leaderboard"
            select (int.Parse(match.Groups["id"].Value), description)
            ;

        return id;
    }

    public async Task<int> GetMemberId()
    {
        (var statusCode, var html) = await GetAsync(null, null, "settings.html", "/settings", true);
        if (statusCode != HttpStatusCode.OK) return 0;

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var text = (from node in document.DocumentNode.SelectNodes("//span")
                    where node.InnerText.Contains("anonymous user #")
                    select node.InnerText).Single();

        return int.Parse(Regex.Match(text, @"#(?<id>\d+)\)").Groups["id"].Value);
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
