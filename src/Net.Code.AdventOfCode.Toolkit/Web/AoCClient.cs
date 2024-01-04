namespace Net.Code.AdventOfCode.Toolkit.Web;

using System.Net;
using Microsoft.Extensions.Logging;
using Net.Code.AdventOfCode.Toolkit.Core;
using System.Linq;
using NodaTime;

class AoCClient : IDisposable, IAoCClient
{
    readonly IClock clock;
    readonly IHttpClientWrapper client;
    private readonly ILogger<AoCClient> logger;

    public AoCClient(IHttpClientWrapper client, IClock clock, ILogger<AoCClient> logger)
    {
        this.clock = clock;
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

        var response = new PostAnswerResult(body).GetResponse();
        return (status, response);
    }

    public async Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id)
    {
        (var statusCode, var content) = await GetAsync($"{year}/leaderboard/private/view/{id}.json");
        if (statusCode != HttpStatusCode.OK || content.StartsWith("<"))
            return null;
        return new LeaderboardJson(content).GetLeaderBoard();
    }

    public async Task<PersonalStats?> GetPersonalStatsAsync(int year)
    {
        var id = await GetMemberId();
        var lb = await GetLeaderBoardAsync(year, id);
        if (lb is null) return null;
        return lb.Members[id];
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
        await VerifyAuthorized();
        return await client.GetAsync(path);
    }

    public async Task<string> GetPuzzleInputAsync(PuzzleKey key)
    {
        (var statusCode, var input) = await GetAsync($"{key.Year}/day/{key.Day}/input");
        if (statusCode != HttpStatusCode.OK) return string.Empty;
        return input;
    }

    public async Task<Puzzle> GetPuzzleAsync(PuzzleKey key)
    {
        HttpStatusCode statusCode;
        (statusCode, var html) = await GetAsync($"{key.Year}/day/{key.Day}");
        if (statusCode != HttpStatusCode.OK) 
            return Puzzle.Locked(key);
        var input = await GetPuzzleInputAsync(key);
        return new PuzzleHtml(key, html, input).GetPuzzle();
    }

    public async Task<IEnumerable<(int id, string description)>> GetLeaderboardIds()
    {
        var year = clock.GetCurrentInstant().ToDateTimeUtc().Year;
        (var statusCode, var html) = await GetAsync($"{year}/leaderboard/private");
        if (statusCode != HttpStatusCode.OK) return Enumerable.Empty<(int, string)>();
        return new LeaderboardHtml(html).GetLeaderboards();
    }

    public async Task<int> GetMemberId()
    {
        (var statusCode, var html) = await GetAsync("settings");
        if (statusCode != HttpStatusCode.OK) return 0;
        return new SettingsHtml(html).GetMemberId();
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
