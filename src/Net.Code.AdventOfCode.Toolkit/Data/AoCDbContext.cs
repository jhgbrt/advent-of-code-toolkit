using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Data;

internal class AoCDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(new SqliteConnectionStringBuilder() { DataSource = @".cache\aoc.db" }.ToString());
        options.LogTo(Console.WriteLine, minimumLevel: Microsoft.Extensions.Logging.LogLevel.Information);
        base.OnConfiguring(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var puzzle = modelBuilder.Entity<Puzzle>();
        puzzle.HasKey(p => new { p.Year, p.Day });
        puzzle.OwnsOne(p => p.Answer);

        var results = modelBuilder.Entity<DayResult>();
        results.HasKey(p => new { p.Year, p.Day });
        results.OwnsOne(p => p.Part1);
        results.OwnsOne(p => p.Part2);

        base.OnModelCreating(modelBuilder);
    }

    internal void Migrate()
    {
        Database.Migrate();
    }

    public void AddPuzzle(Puzzle puzzle)
    {
        Puzzles.Add(puzzle);
        SaveChanges();
    }

    public void SaveResult(DayResult result)
    {
        var item = Results.FirstOrDefault(p => p.Year == result.Year && p.Day == result.Day);
        if (item is null)
            Results.Add(result);
        else
            Results.Update(result);
        SaveChanges();
    }

    public DbSet<Puzzle> Puzzles { get; set; }
    public DbSet<DayResult> Results { get; set; }

}
