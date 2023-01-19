
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Show some stats from the configured private leaderboard. Set AOC_LEADERBOARD_ID as a environment variable.")]
class Stats : AsyncCommand<AoCSettings>
{
    private readonly ILeaderboardManager manager;
    private readonly IInputOutputService io;
    private readonly AoCLogic logic;
    public Stats(ILeaderboardManager manager, IInputOutputService io, AoCLogic logic)
    {
        this.manager = manager;
        this.io = io;
        this.logic = logic;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AoCSettings _)
    {

        await foreach (var m in manager.GetMemberStats(logic.Years()))
        {
            io.WriteLine($"{m.year}: {m.stars}, {m.score}");
        }

        return 0;
    }
}
