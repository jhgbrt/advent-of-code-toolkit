namespace Net.Code.AdventOfCode.Toolkit.Core;

class DayResult : IHavePuzzleKey
{
    public PuzzleKey Key { get; init; }
    public int Year { get; init; }
    public int Day { get; init; }
    public Result Part1 { get; set; }
    public Result Part2 { get; set; }
    public DayResult(PuzzleKey key, Result part1, Result part2)
    {
        Key = key;
        Year = key.Year;
        Day = key.Day;
        Part1 = part1;
        Part2 = part2;
    }

    private DayResult()
    {
        Part1 = Result.Empty;
        Part2 = Result.Empty;
    }

    public static DayResult NotImplemented(PuzzleKey key) => new DayResult(key, Result.Empty, Result.Empty);
    public TimeSpan Elapsed { get; init; }

    public override string ToString()
    {
        return $"{Key} (year {Year}, day {Day}) - Elapsed = {Elapsed}; Part1 = {Part1}, Part2 = {Part2}";
    }
}
