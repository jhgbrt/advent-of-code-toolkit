using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Web;

using Spectre.Console.Cli;

using System.Text.Json;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Migrate : AsyncCommand<Migrate.Settings>
{
    public class Settings : CommandSettings { }
    private readonly ICache cache;
    private readonly IAoCDbContext dbcontext;
    private readonly AoCLogic AoCLogic;

    public Migrate(IAoCDbContext dbcontext, AoCLogic aoCLogic, ICache cache)
    {
        this.dbcontext = dbcontext;
        this.AoCLogic = aoCLogic;
        this.cache = cache;
    }
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        dbcontext.Migrate();
        foreach (var key in AoCLogic.Puzzles())
        {
            Console.WriteLine(key);

            var puzzle = await GetPuzzle(key);
            if (puzzle != null)
                await UpsertPuzzle(key, puzzle);

            var result = await GetResult(key);
            if (result  != null)
                await UpsertResult(key, result);

        }
        return 0;
    }
    private async Task<Puzzle> GetPuzzle(PuzzleKey key)
    {
        var html = await cache.ReadFromCache(key.Year, key.Day, "puzzle.html");
        var input = await cache.ReadFromCache(key.Year, key.Day, "input.txt");
        return new PuzzleHtml(key, html, input).GetPuzzle();
    }

    private async Task<DayResultV1?> GetResult(PuzzleKey key)
    {
        var json = await cache.ReadFromCache(key.Year, key.Day, "result.json");
        if (json != null)
        {
            return JsonSerializer.Deserialize<DayResultV1>(json);
        }
        return null;
    }

    private async Task UpsertPuzzle(PuzzleKey key, Puzzle puzzle)
    {
        var existing = await dbcontext.GetPuzzle(key);
        if (existing != null)
        {
            existing.Answer = puzzle.Answer;
            existing.Status = puzzle.Status;
        }
        else
        {
            dbcontext.AddPuzzle(puzzle);
        }
    }

    private async Task UpsertResult(PuzzleKey key, DayResultV1 result)
    {
        var existing = await dbcontext.GetResult(key);
        if (existing != null)
        {
            existing.Part1 = result.part1;
            existing.Part2 = result.part2;
        }
        else
        {
            var newresult = new DayResult(key, result.part1, result.part2);
            dbcontext.AddResult(newresult);
        }
    }
}
