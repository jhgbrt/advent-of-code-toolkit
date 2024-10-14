
using Microsoft.EntityFrameworkCore;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Text;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class PuzzleManager(IAoCClient client, IAoCDbContext db, AoCLogic logic) : IPuzzleManager
{
    public async Task<Puzzle> GetPuzzle(PuzzleKey key)
    {
        logic.EnsureValid(key);

        var puzzle = await db.GetPuzzle(key);


        if (puzzle == null)
        {
            puzzle = await SyncPuzzle(key);
        }

        if (puzzle == null)
        {
            throw new Exception($"Puzzle {key} not initialized on this machine. If this puzzle was solved on another machine, use sync first.");
        }

        return puzzle;
    }

    public async Task<Puzzle> SyncPuzzle(PuzzleKey key)
    {
        logic.EnsureValid(key);
        var puzzle = await db.GetPuzzle(key);
        var remote = await client.GetPuzzleAsync(key);

        if (puzzle == null)
        {
            puzzle = remote;
            db.AddPuzzle(puzzle);
        }
        else
        {
            puzzle.UpdateFrom(remote);
        }
        await db.SaveChangesAsync();
        return puzzle;
    }

    public async Task<Puzzle[]> GetPuzzles()
    {
        return await db.Puzzles.ToArrayAsync();
    }
    public async Task<PuzzleResultStatus[]> GetPuzzleResults(int? year, TimeSpan? slowerthan)
    {
        var puzzles = db.Puzzles.AsNoTracking().AsQueryable();
        var results = db.Results.AsNoTracking().AsQueryable();
        if (year.HasValue)
        {
            puzzles = puzzles.Where(p => p.Year == year.Value);
            results = results.Where(r => r.Year == year.Value);
        }
        if (slowerthan.HasValue)
        {
            results = results.Where(r => r.Elapsed > slowerthan.Value);
        }

        var query = await (from puzzle in puzzles
                     join result in results on puzzle.Key equals result.Key into g
                     from result in g.DefaultIfEmpty()
                     select new { puzzle, result }
                     ).ToArrayAsync();

        return (from item in query
                let puzzle = item.puzzle
                let result = item.result ?? DayResult.NotImplemented(puzzle.Key)
                select new PuzzleResultStatus(puzzle, result)).ToArray();
    }
    public async Task<DayResult[]> GetPuzzleResults(int? slowerthan)
    {
        var queryable = db.Results.AsQueryable();
        if (slowerthan.HasValue)
        {
            var min = TimeSpan.FromSeconds(slowerthan.Value);
            queryable = queryable.Where(r => r.Part1.Elapsed + r.Part2.Elapsed>= min);
        }

        return await queryable.ToArrayAsync();
    }

    public async Task<PuzzleResultStatus> GetPuzzleResult(PuzzleKey key)
    {
        logic.EnsureValid(key);

        var puzzle = await db.GetPuzzle(key);

        if (puzzle is null)
            throw new ArgumentException(nameof(key));

        var result = await db.GetResult(key)
            ?? DayResult.NotImplemented(key);

        return new PuzzleResultStatus(puzzle, result);
    }

    public async Task<(bool success, string content)> PostAnswer(PuzzleKey key, string value)
    {
        logic.EnsureValid(key);

        var puzzle = await GetPuzzle(key);

        var answer = puzzle.CreateAnswer(value);

        var (_, content) = await client.PostAnswerAsync(key.Year, key.Day, answer.part, answer.value);

        var success = content.StartsWith("That's the right answer");

        if (success)
        {
            puzzle.SetAnswer(answer);

            var stats = await client.GetPersonalStatsAsync(key.Year);
            content = new StringBuilder(content).AppendLine().AppendLine($"You now have {stats?.TotalStars} stars and a score of {stats?.LocalScore}").ToString();
        }
        return (success, content);
    }

    public async Task AddResult(DayResult result)
    {
        logic.EnsureValid(result.Key);

        var current = await db.GetResult(result.Key);
        if (current != null)
        {
            current.Part1 = result.Part1;
            current.Part2 = result.Part2;
        }
        else
        {
            db.AddResult(result);
        }
        await db.SaveChangesAsync();
    }
}

