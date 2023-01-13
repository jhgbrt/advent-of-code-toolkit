
using Net.Code.AdventOfCode.Toolkit.Core;

using System.Net;
using System.Text;
using System.Text.Json;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class PuzzleManager : IPuzzleManager
{
    private readonly IAoCClient client;
    private readonly ICache cache;

    public PuzzleManager(IAoCClient client, ICache cache)
    {
        this.client = client;
        this.cache = cache;
    }

    public async Task<(bool status, string reason, int part)> PreparePost(int year, int day)
    {
        var puzzle = await client.GetPuzzleAsync(year, day);

        return puzzle.Status switch
        {
            Status.Locked => (false, "Puzzle is locked. Did you initialize it?", 0),
            Status.Completed => (false, "Already completed", 0),
            _ => (true, string.Empty, puzzle.Status == Status.Unlocked ? 1 : 2)
        };
    }

    public async Task<Puzzle> GetPuzzle(int y, int d)
    {
        var puzzle = await client.GetPuzzleAsync(y, d);
        return puzzle;
    }
    public async Task<DayResult> GetPuzzleResult(int y, int d)
    {
        return cache.Exists(y, d, "result.json")
            ? JsonSerializer.Deserialize<DayResult>(await cache.ReadFromCache(y, d, "result.json"))!
            : DayResult.NotImplemented(y, d);
    }

    public async Task<(bool success, HttpStatusCode status, string content)> Post(int year, int day, int part, string value)
    {
        var (status, content) = await client.PostAnswerAsync(year, day, part, value);
        var success = content.StartsWith("That's the right answer");


        if (success)
        {
            // when a new result is posted successfully, we need to update the puzzle data
            await client.GetPuzzleAsync(year, day, false);
            var stats = await client.GetMemberAsync(year, false);
            content = new StringBuilder(content).AppendLine().AppendLine($"You now have {stats?.TotalStars} stars and a score of {stats?.LocalScore}").ToString();
        }
        return (success, status, content);
    }

    public async Task SaveResult(DayResult result)
    {
        await cache.WriteToCache(result.Year, result.Day, "result.json", JsonSerializer.Serialize(result));
    }
}

