<<<<<<< HEAD
﻿
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;

using Spectre.Console.Cli;

using System.Text.Json;

using Spectre.Console.Cli;

=======
﻿using Net.Code.AdventOfCode.Toolkit.Core;

using Spectre.Console.Cli;

>>>>>>> 780ecea (remove cache)
namespace Net.Code.AdventOfCode.Toolkit.Commands;

class Migrate : AsyncCommand<Migrate.Settings>
{
    public class Settings : CommandSettings { }
    private readonly IAoCDbContext dbcontext;
<<<<<<< HEAD
=======

>>>>>>> 780ecea (remove cache)
    public Migrate(IAoCDbContext dbcontext)
    {
        this.dbcontext = dbcontext;
    }
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        dbcontext.Migrate();
        return 0;
    }
}
