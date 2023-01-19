﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Data;
public class TimeSpanConverter : ValueConverter<TimeSpan, long>
{
    public TimeSpanConverter()
        : base(
            v => v.Ticks,
            v => TimeSpan.FromTicks(v))
    {
    }
}

internal class AoCDbContextFactory : IDesignTimeDbContextFactory<AoCDbContext>
{
    public AoCDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AoCDbContext>();
        optionsBuilder.UseSqlite(@"Data Source=.cache\aoc.db");

        return new AoCDbContext(optionsBuilder.Options);
    }
}

internal interface IAoCDbContext
{
    void AddPuzzle(Puzzle puzzle);
    void AddResult(DayResult result);
    ValueTask<Puzzle?> GetPuzzle(PuzzleKey key);
    ValueTask<DayResult?> GetResult(PuzzleKey key);
    void Migrate();
    Task<int> SaveChangesAsync(CancellationToken token = default);

    IQueryable<Puzzle> Puzzles { get; }
    IQueryable<DayResult> Results { get; }
}

internal class AoCDbContext : DbContext, IAoCDbContext
{
    public AoCDbContext(DbContextOptions<AoCDbContext> options) : base(options) { }
    public AoCDbContext() { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TimeSpan>().HaveConversion<TimeSpanConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var results = modelBuilder.Entity<DayResult>();
        ConfigureKey(results);
        results.OwnsOne(p => p.Part1);
        results.OwnsOne(p => p.Part2);
        results.Property(p => p.Elapsed).HasComputedColumnSql("Part1_Elapsed + Part2_Elapsed");

        var puzzles = modelBuilder.Entity<Puzzle>();
        ConfigureKey(puzzles);
        puzzles.OwnsOne(p => p.Answer);

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureKey<T>(EntityTypeBuilder<T> entity) where T : class, IHavePuzzleKey
    {
        var converter = new ValueConverter<PuzzleKey, int>(
               v => v.Id,
               v => new PuzzleKey(v));
        entity.HasKey(p => p.Key);
        entity.Property(p => p.Key).HasConversion(converter);
        entity.Property(p => p.Year).HasComputedColumnSql("Key/100");
        entity.Property(p => p.Day).HasComputedColumnSql("Key%100");
        entity.HasIndex(nameof(IHavePuzzleKey.Year), nameof(IHavePuzzleKey.Day));
    }

    public void Migrate()
    {
        Database.Migrate();
    }

    public void AddPuzzle(Puzzle puzzle) => Puzzles.Add(puzzle);
    public void AddResult(DayResult result) => Results.Add(result);
    public ValueTask<Puzzle?> GetPuzzle(PuzzleKey key) => Puzzles.FindAsync(key);
    public ValueTask<DayResult?> GetResult(PuzzleKey key) => Results.FindAsync(key);

    public DbSet<Puzzle> Puzzles { get; set; }
    public DbSet<DayResult> Results { get; set; }

    IQueryable<Puzzle> IAoCDbContext.Puzzles => this.Puzzles;

    IQueryable<DayResult> IAoCDbContext.Results => this.Results;
}
