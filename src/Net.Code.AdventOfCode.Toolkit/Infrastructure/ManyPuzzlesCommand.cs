using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

abstract class ManyPuzzlesCommand<TSettings> : AsyncCommand<TSettings> where TSettings : CommandSettings, IManyPuzzleSettings
{
    private readonly AoCLogic AoCLogic;

    protected ManyPuzzlesCommand(AoCLogic aoCLogic)
    {
        AoCLogic = aoCLogic;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TSettings options)
    {
        var (year, day, all) = (options.year, options.day, options.all);
        int result = 0;
        foreach (var (y, d) in AoCLogic.Puzzles(year, day, all))
        {
            var key = new PuzzleKey(y, d);
            var v = await ExecuteAsync(key, options);
            if (v != 0)
                result = v;
        }
        return result;
    }

    public abstract Task<int> ExecuteAsync(PuzzleKey key, TSettings options);

}
