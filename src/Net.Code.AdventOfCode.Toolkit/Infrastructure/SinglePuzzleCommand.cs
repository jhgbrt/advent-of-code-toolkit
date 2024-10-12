using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

abstract class SinglePuzzleCommand<TSettings>(AoCLogic aoCLogic) : AsyncCommand<TSettings> where TSettings : CommandSettings, IAoCSettings
{
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings options)
    {
        (var year, var day) = (options.year, options.day);
        if (!year.HasValue || !day.HasValue)
        {
            year = aoCLogic.Year;
            day = aoCLogic.Day;
        }

        if (!year.HasValue || !day.HasValue)
            throw new AoCException("Please specify year & day explicitly");

        if (!aoCLogic.IsValidAndUnlocked(year.Value, day.Value))
            throw new AoCException($"Not a valid puzzle, or puzzle not yet unlocked: {year}/{day}");

        return await ExecuteAsync(new PuzzleKey(year.Value, day.Value), options);
    }

    public abstract Task<int> ExecuteAsync(PuzzleKey key, TSettings options);

}
