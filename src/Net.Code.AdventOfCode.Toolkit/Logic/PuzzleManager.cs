
using Microsoft.EntityFrameworkCore;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;

using System.Text;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class PuzzleManager : IPuzzleManager
{
    private readonly IAoCClient client;
    private readonly AoCDbContext db;

    public PuzzleManager(IAoCClient client, AoCDbContext db)
    {
        this.client = client;
        this.db = db;
    }

    public async Task<(bool status, string reason, int part)> PreparePost(int year, int day)
    {
        var puzzle = await GetPuzzle(year, day);
       
        return puzzle.Status switch
        {
            Status.Locked => (false, "Puzzle is locked. Did you initialize it?", 0),
            Status.Completed => (false, "Already completed", 0),
            _ => (true, string.Empty, puzzle.Status == Status.Unlocked ? 1 : 2)
        };
    }

    public async Task<Puzzle> GetPuzzle(int y, int d)
    {
        var puzzle = db.Puzzles.Find(new PuzzleKey(y,d));

        if (puzzle == null)
        {
            puzzle = await client.GetPuzzleAsync(y, d);
            db.AddPuzzle(puzzle);
        }

        return puzzle;
    }
    public async Task<Puzzle[]> GetPuzzles()
    {
        return await db.Puzzles.ToArrayAsync();
    }
    public async Task<(Puzzle, DayResult)[]> GetPuzzlesWithResults(int? year, TimeSpan? slowerthan)
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
                select (puzzle, result)).ToArray();
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

    public async Task<DayResult> GetPuzzleResult(int y, int d)
    {
        var key = new PuzzleKey(y, d);
        return await db.Results.FindAsync(key)
            ?? DayResult.NotImplemented(y, d);
    }

    public async Task<(bool success, string content)> Post(int year, int day, int part, string value)
    {
        var (_, content) = await client.PostAnswerAsync(year, day, part, value);
        var success = content.StartsWith("That's the right answer");


        if (success)
        {
            // when a new result is posted successfully, we need to update the puzzle data
            var updated = await client.GetPuzzleAsync(year, day);

            var puzzle = await GetPuzzle(year, day);
            puzzle.Answer = updated.Answer;
            puzzle.Status = updated.Status;

            var stats = await client.GetMemberAsync(year, false);
            content = new StringBuilder(content).AppendLine().AppendLine($"You now have {stats?.TotalStars} stars and a score of {stats?.LocalScore}").ToString();
        }
        return (success, content);
    }

    public async Task SaveResult(DayResult result)
    {
        var current = await db.Results.FirstOrDefaultAsync(r => r.Year == result.Year && r.Day == result.Day);
        if (current != null)
        {
            current.Part1 = result.Part1;
            current.Part2 = result.Part2;
        }
        else
        {
            await db.Results.AddAsync(result);
        }
    }
}

