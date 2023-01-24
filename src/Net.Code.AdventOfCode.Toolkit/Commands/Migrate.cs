using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Migrate : AsyncCommand<Migrate.Settings>
{
    public class Settings : CommandSettings { }
    private readonly IAoCDbContext dbcontext;

    public Migrate(IAoCDbContext dbcontext)
    {
        this.dbcontext = dbcontext;
    }
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        dbcontext.Migrate();
        return Task.FromResult(0);
    }
}
