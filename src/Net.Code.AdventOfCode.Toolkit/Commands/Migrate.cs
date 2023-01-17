using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;

using Spectre.Console.Cli;

using System.Text.Json;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Migrate : AsyncCommand<Migrate.Settings>
{
    public class Settings : CommandSettings { }
    private readonly IAoCClient aocclient;
    private readonly ICache cache;
    private readonly AoCDbContext dbcontext;
    private readonly AoCLogic AoCLogic;

    public Migrate(AoCDbContext dbcontext, IAoCClient client, AoCLogic aoCLogic, ICache cache)
    {
        this.dbcontext = dbcontext;
        this.aocclient = client;
        this.AoCLogic = aoCLogic;
        this.cache = cache;
    }
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        dbcontext.Migrate();
        foreach (var (y, d) in AoCLogic.Puzzles())
        {
            Console.WriteLine((y,d));

            var key = new PuzzleKey(y, d);
            var puzzle = await GetPuzzle(y, d);
            if (puzzle != null)
                await UpsertPuzzle(key, puzzle);

            var result = await GetResult(y, d);
            if (result  != null)
                await UpsertResult(key, result);

        }
        return 0;
    }
    private async Task<Puzzle> GetPuzzle(int year, int day)
    {
        var html = await cache.ReadFromCache(year, day, "puzzle.html");
        var input = await cache.ReadFromCache(year, day, "input.txt");
        return new PuzzleHtml(year, day, html, input).GetPuzzle();
    }

    private async Task<DayResultV1?> GetResult(int y, int d)
    {
        var json = await cache.ReadFromCache(y, d, "result.json");
        if (json != null)
        {
            return JsonSerializer.Deserialize<DayResultV1>(json);
        }
        return null;
    }

    private async Task UpsertPuzzle(PuzzleKey key, Puzzle puzzle)
    {
        var existing = await dbcontext.Puzzles.FindAsync(key);
        if (existing != null)
        {
            existing.Answer = puzzle.Answer;
            existing.Status = puzzle.Status;
        }
        else
        {
            dbcontext.Puzzles.Add(puzzle);
        }
    }

    private async Task UpsertResult(PuzzleKey key, DayResultV1 result)
    {
        var existing = await dbcontext.Results.FindAsync(key);
        if (existing != null)
        {
            existing.Part1 = result.part1;
            existing.Part2 = result.part2;
        }
        else
        {
            var newresult = new DayResult(key, result.part1, result.part2);
            dbcontext.Results.Add(newresult);
        }
    }
}
