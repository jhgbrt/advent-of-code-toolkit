using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Net.Code.AdventOfCode.Toolkit.Data;

internal class AoCDbContextFactory : IDesignTimeDbContextFactory<AoCDbContext>
{
    public AoCDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AoCDbContext>();
        optionsBuilder.UseSqlite(@"Data Source=.cache\aoc.db");

        return new AoCDbContext(optionsBuilder.Options);
    }
}
