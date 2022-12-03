
using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Show some stats from the configured private leaderboard. Set AOC_LEADERBOARD_ID as a environment variable.")]
class Stats : AsyncCommand<AoCSettings>
{
    private readonly IReportManager manager;
    private readonly IInputOutputService io;

    public Stats(IReportManager manager, IInputOutputService io)
    {
        this.manager = manager;
        this.io = io;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AoCSettings _)
    {

        await foreach (var (year, m) in manager.GetMemberStats())
        {
            io.WriteLine($"{year}: {m.stars}, {m.score}");
        }

        return 0;
    }
}
