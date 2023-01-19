
using Microsoft.EntityFrameworkCore;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;

using System.Text;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class PuzzleManager : IPuzzleManager
{
    private readonly IAoCClient client;
    private readonly IAoCDbContext db;

    public PuzzleManager(IAoCClient client, IAoCDbContext db)
    {
        this.client = client;
        this.db = db;
    }

    public async Task<(bool status, string reason, int part)> PreparePost(PuzzleKey key)
    {
        var puzzle = await GetPuzzle(key);
       
        return puzzle.Status switch
        {
            Status.Locked => (false, "Puzzle is locked. Did you initialize it?", 0),
            Status.Completed => (false, "Already completed", 0),
            _ => (true, string.Empty, puzzle.Status == Status.Unlocked ? 1 : 2)
        };
    }

    public async Task<Puzzle> GetPuzzle(PuzzleKey key)
    {
        var puzzle = await db.GetPuzzle(key);

        if (puzzle == null)
        {
            puzzle = await client.GetPuzzleAsync(key);
            db.AddPuzzle(puzzle);
        }

        return puzzle;
    }
    public async Task<Puzzle[]> GetPuzzles()
    {
        return await db.Puzzles.ToArrayAsync();
    }
    public async Task<PuzzleResultStatus[]> GetPuzzleResults(int? year, TimeSpan? slowerthan)
    {
        var puzzles = db.Puzzles.AsNoTracking().AsQueryable();
        if (year.HasValue)
        {
            puzzles = puzzles.Where(p => p.Year == year.Value);
        }
        var results = db.Results.AsNoTracking().AsQueryable();
        if (slowerthan.HasValue)
            results = results.Where(r => r.Elapsed > slowerthan.Value);


        var query = await (from puzzle in puzzles
                     join result in results on puzzle.Key equals result.Key into g
                     from result in g.DefaultIfEmpty()
                     select new { puzzle, result }
                     ).ToArrayAsync();

        return (from item in query
                let puzzle = item.puzzle
                let result = item.result ?? DayResult.NotImplemented(puzzle.Year, puzzle.Day)
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
        var puzzle = await db.GetPuzzle(key);
        if (puzzle is null)
            throw new ArgumentException(nameof(key));
        var result = await db.GetResult(key)
            ?? DayResult.NotImplemented(key);
        return new PuzzleResultStatus(puzzle, result);
    }

    public async Task<(bool success, string content)> PostAnswer(PuzzleKey key, AnswerToPost answer)
    {
        var (_, content) = await client.PostAnswerAsync(key.Year, key.Day, answer.part, answer.value);
        var success = content.StartsWith("That's the right answer");


        if (success)
        {
            var puzzle = await GetPuzzle(key);
            puzzle.SetAnswer(answer);

            var stats = await client.GetMemberAsync(key.Year);
            content = new StringBuilder(content).AppendLine().AppendLine($"You now have {stats?.TotalStars} stars and a score of {stats?.LocalScore}").ToString();
        }
        return (success, content);
    }

    public async Task SaveResult(DayResult result)
    {
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
    }
}

