
using Microsoft.EntityFrameworkCore;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;

using System.Net;
using System.Text;
using System.Text.Json;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class PuzzleManager : IPuzzleManager
{
    private readonly IAoCClient client;
    private readonly AoCDbContext cache;

    public PuzzleManager(IAoCClient client, AoCDbContext cache)
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
        return await cache.Results.FirstOrDefaultAsync(r => r.Year == y && r.Day == d)
            ?? DayResult.NotImplemented(y, d);
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
        var current = await cache.Results.FirstOrDefaultAsync(r => r.Year == result.Year && r.Day == result.Day);
        if (current != null)
        {
            current.Part1 = result.Part1;
            current.Part2 = result.Part2;
        }
        else
        {
            await cache.Results.AddAsync(result);
        }
        await cache.SaveChangesAsync();
    }
}

