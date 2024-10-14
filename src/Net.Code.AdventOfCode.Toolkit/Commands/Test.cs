using Net.Code.AdventOfCode.Toolkit.Infrastructure;

using Spectre.Console.Cli;

using System.Diagnostics;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Test : AsyncCommand<AoCSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, AoCSettings settings)
    {
        Debugger.Break();
        return Task.FromResult(0);
    }
}