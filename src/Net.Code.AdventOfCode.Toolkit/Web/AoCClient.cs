namespace Net.Code.AdventOfCode.Toolkit.Web;

using System.Net;
using Microsoft.Extensions.Logging;
using Net.Code.AdventOfCode.Toolkit.Core;

class AoCClient(IHttpClientWrapper client, ILogger<AoCClient> logger) : IAoCClient
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
        (var status, var body) = await client.PostAsync($"{year}/day/{day}/answer", content);

        var response = new PostAnswerResult(body).GetResponse();
        return (status, response);
    }

    public async Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id)
    {
        (var statusCode, var content) = await client.GetAsync($"{year}/leaderboard/private/view/{id}.json");
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


    public async Task<string> GetPuzzleInputAsync(PuzzleKey key)
    {
        (var statusCode, var input) = await client.GetAsync($"{key.Year}/day/{key.Day}/input");
        if (statusCode != HttpStatusCode.OK) return string.Empty;
        return input;
    }

    public async Task<Puzzle> GetPuzzleAsync(PuzzleKey key)
    {
        HttpStatusCode statusCode;
        (statusCode, var html) = await client.GetAsync($"{key.Year}/day/{key.Day}");
        if (statusCode != HttpStatusCode.OK) 
            return Puzzle.Locked(key);
        var input = await GetPuzzleInputAsync(key);
        return new PuzzleHtml(key, html, input).GetPuzzle();
    }

    public async Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(int year)
    {
        (var statusCode, var html) = await client.GetAsync($"{year}/leaderboard/private");
        if (statusCode != HttpStatusCode.OK) return [];
        return new LeaderboardHtml(html).GetLeaderboards();
    }

    public async Task<int> GetMemberId()
    {
        (var statusCode, var html) = await client.GetAsync("settings");
        if (statusCode != HttpStatusCode.OK) return 0;
        return new SettingsHtml(html).GetMemberId();
    }

}
