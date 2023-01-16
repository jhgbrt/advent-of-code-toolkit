
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;
using Net.Code.AdventOfCode.Toolkit.Migrations;

using Spectre.Console.Cli;

using System.Text.Json;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Migrate : AsyncCommand<Migrate.Settings>
{
    public class Settings : CommandSettings { }
    private readonly IAoCClient aocclient;
    private readonly ICache cache;
    private readonly IPuzzleManager puzzleManager;
    private readonly AoCDbContext dbcontext;
    private readonly AoCLogic AoCLogic;

    public Migrate(AoCDbContext dbcontext, IAoCClient client, AoCLogic aoCLogic, IPuzzleManager puzzleManager, ICache cache)
    {
        this.dbcontext = dbcontext;
        this.aocclient = client;
        this.AoCLogic = aoCLogic;
        this.puzzleManager = puzzleManager;
        this.cache = cache;
    }
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        dbcontext.Migrate();
        foreach (var (y, d) in AoCLogic.Puzzles())
        {
            Console.WriteLine((y,d));
            var json = await cache.ReadFromCache(y, d, "result.json");
            if (json != null)
            {
                var result = JsonSerializer.Deserialize<DayResult>(json)!;
                Console.WriteLine((result.Year, result.Day));
                dbcontext.Results.Add(result);
            }
        }
        await dbcontext.SaveChangesAsync();

        return 0;
    }
}
