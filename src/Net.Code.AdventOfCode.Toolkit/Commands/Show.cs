namespace Net.Code.AdventOfCode.Toolkit.Commands;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.ComponentModel;
using System.Diagnostics;

[Description("Show the puzzle instructions.")]
class Show : SinglePuzzleCommand<AoCSettings>
{
    private readonly Configuration configuration;

    public Show(Configuration configuration, AoCLogic logic) : base(logic)
    {
        this.configuration = configuration;
    }

    public override Task<int> ExecuteAsync(int year, int day, AoCSettings options)
    {
        ProcessStartInfo psi = new()
        {
            FileName = $"{configuration.BaseAddress}/{year}/day/{day}",
            UseShellExecute = true
        };
        Process.Start(psi);
        return Task.FromResult(0);
    }
}
