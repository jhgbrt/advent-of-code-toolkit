namespace Net.Code.AdventOfCode.Toolkit.Web;

using System.Net;
using Microsoft.Extensions.Logging;
using Net.Code.AdventOfCode.Toolkit.Core;

class AoCClient(HttpClient client, ILogger<AoCClient> logger) : IAoCClient
{
    public async Task<(HttpStatusCode status, string content)> PostAnswerAsync(int year, int day, int part, string value)
    {
        logger.LogDebug("Posting answer for {year} - {day}: {value}", year, day, value);

        var formValues = new Dictionary<string, string>()
        {
            ["level"] = part.ToString(),
            ["answer"] = value
        };
        var content = new FormUrlEncodedContent(formValues);
        (var status, var body) = await PostAsync($"{year}/day/{day}/answer", content);

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

    public async Task<Puzzle> GetPuzzleAsync(PuzzleKey key)
    {
        HttpStatusCode statusCode;
        (statusCode, var html) = await GetAsync($"{key.Year}/day/{key.Day}");
        if (statusCode != HttpStatusCode.OK) 
            return Puzzle.Locked(key);
        (statusCode, var input) = await GetAsync($"{key.Year}/day/{key.Day}/input");
        if (statusCode != HttpStatusCode.OK) input = string.Empty;
        return new PuzzleHtml(key, html, input).GetPuzzle();
    }

    public async Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(int year)
    {
        (var statusCode, var html) = await GetAsync($"{year}/leaderboard/private");
        if (statusCode != HttpStatusCode.OK) return [];
        return new LeaderboardHtml(html).GetLeaderboards();
    }

    public async Task<int> GetMemberId()
    {
        (var statusCode, var html) = await GetAsync("settings");
        if (statusCode != HttpStatusCode.OK) return 0;
        return new SettingsHtml(html).GetMemberId();
    }


    Dictionary<string, (HttpStatusCode statusCode, string content)> cache = [];
    bool authenticated;
    private async Task EnsureAuthenticated()
    {
        if (authenticated) return;
        var response = await client.GetAsync($"{2015}/day/4/input");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Unauthorized");
            throw new NotAuthenticatedException("This command requires logging in. The AOC_SESSION cookie is set, but may be expired. " +
                "Log in to adventofcode.com, and find the 'session' cookie value in your browser devtools. " +
                "Copy this value and set it as an environment variable in your shell, or as a dotnet user-secret for your project.");
        }
        authenticated = true;
    }
    public async Task<(HttpStatusCode status, string content)> PostAsync(string path, HttpContent body)
    {
        await EnsureAuthenticated();
        var response = await client.PostAsync(path, body);
        logger.LogTrace("POST: {path} - {statusCode}", path, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        return (response.StatusCode, content);
    }
    public async Task<(HttpStatusCode status, string content)> GetAsync(string path)
    {
        if (cache.ContainsKey(path)) return cache[path];
        await EnsureAuthenticated();
        var response = await client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("GET: {path} returned {statusCode}", path, response.StatusCode);
            logger.LogTrace("GET: {path} returned {statusCode}: {content}", path, response.StatusCode, content);
        }
        else
        {
            logger.LogTrace("GET: {path} - {statusCode}", path, response.StatusCode);
        }
        cache[path] = (response.StatusCode, content);
        return (response.StatusCode, content);
    }
}
