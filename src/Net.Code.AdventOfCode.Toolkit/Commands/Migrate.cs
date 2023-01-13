
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Migrate : AsyncCommand<Migrate.Settings>
{
    public class Settings : CommandSettings { }
    private readonly IAoCClient aocclient;
    private readonly IPuzzleManager puzzleManager;
    private readonly AoCDbContext dbcontext;
    private readonly AoCLogic AoCLogic;

    public Migrate(AoCDbContext dbcontext, IAoCClient client, AoCLogic aoCLogic, IPuzzleManager puzzleManager)
    {
        this.dbcontext = dbcontext;
        this.aocclient = client;
        this.AoCLogic = aoCLogic;
        this.puzzleManager = puzzleManager;
    }
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        dbcontext.Migrate();
        foreach (var (y, d) in AoCLogic.Puzzles())
        {
            Console.WriteLine((y,d));
            var result = await puzzleManager.GetPuzzleResult(y, d, (i, r) => { });
            Console.WriteLine((result.result.Year, result.result.Day));
            dbcontext.Results.Add(result.result);
        }
        await dbcontext.SaveChangesAsync();

        //var puzzle = dbcontext.Puzzles.First(p => p.Year == 2018 && p.Day == 01);
        //Console.WriteLine(puzzle.Text);

        return 0;
    }
}
