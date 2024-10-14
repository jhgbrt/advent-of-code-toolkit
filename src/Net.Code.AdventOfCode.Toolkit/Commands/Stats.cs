
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Show some stats from the configured private leaderboard. Set AOC_LEADERBOARD_ID as a environment variable.")]
class Stats(ILeaderboardManager manager, IInputOutputService io, AoCLogic logic) : AsyncCommand<AoCSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AoCSettings _)
    {

        await foreach (var m in manager.GetMemberStats(logic.Years()))
        {
            io.WriteLine($"{m.year}: {m.stars}, {m.score}");
        }

        return 0;
    }
}
