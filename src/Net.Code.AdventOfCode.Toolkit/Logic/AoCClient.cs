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

    public AoCClient(IHttpClientWrapper client, ILogger<AoCClient> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<(HttpStatusCode status, string content)> PostAnswerAsync(int year, int day, int part, string value)
    {
        await VerifyAuthorized();

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

    public async Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id)
    {
        (var statusCode, var content) = await GetAsync($"{year}/leaderboard/private/view/{id}.json");
        if (statusCode != HttpStatusCode.OK || content.StartsWith("<"))
            return null;
        return Deserialize(year, content);
    }

    public async Task<Member?> GetMemberAsync(int year, bool usecache = true)
    {
        var id = await GetMemberId();
        var lb = await GetLeaderBoardAsync(year, id);
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
    private async Task VerifyAuthorized()
    {
        // not all endpoints return the correct status code
        // therefore we request a small input, this endpoint does give 404 when not authorized.
        (var status, _) = await client.GetAsync($"{2015}/day/4/input");
        if (status == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException("You are not logged in. This probably means you need to renew your AOC_SESSION cookie. Log in on adventofcode.com, copy the session cookie and set it as a user secret or environment variable.");
    }
    private async Task<(HttpStatusCode StatusCode, string Content)> GetAsync(string path)
    {
        string content;
        await VerifyAuthorized();
        (var status, content) = await client.GetAsync(path);
        return (status, content);
    }

    public async Task<string> GetPuzzleInputAsync(int year, int day)
    {
        (var statusCode, var input) = await GetAsync($"{year}/day/{day}/input");
        if (statusCode != HttpStatusCode.OK) return string.Empty;
        return input;
    }

    public async Task<Puzzle> GetPuzzleAsync(int year, int day)
    {
        HttpStatusCode statusCode;
        (statusCode, var html) = await GetAsync($"{year}/day/{day}");
        if (statusCode != HttpStatusCode.OK) return Puzzle.Locked(year, day);
        var input = await GetPuzzleInputAsync(year, day);
        return new PuzzleHtml(year, day, html, input).GetPuzzle();
    }

    public async Task<IEnumerable<(int id, string description)>> GetLeaderboardIds()
    {
        var year = DateTime.Now.Year;
        (var statusCode, var html) = await GetAsync($"{year}/leaderboard/private");
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
        (var statusCode, var html) = await GetAsync("/settings");
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
