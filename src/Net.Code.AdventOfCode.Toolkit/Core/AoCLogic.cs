namespace Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

class AoCLogic(IClock clock)
{
    public AoCLogic() : this(SystemClock.Instance) { }
    public IClock Clock { get; } = clock;
    ZonedDateTime Now => Clock.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb["EST"]);
    bool InAdvent => Now.Month == 12 && Now.Day <= 25;
    public int? Year => Now.Month == 12 ? Now.Year : null;
    public int? Day => Now.Month == 12 && Now.Day >= 1 && Now.Day <= 25 ? Now.Day : null;
    public IEnumerable<PuzzleKey> Puzzles()
        => from year in Years() from day in Days(year) select new PuzzleKey(year, day);
    public IEnumerable<(int year, int day)> Puzzles(int? year, int? day, bool all = false)
    {

        (year, day) = (year, day, all) switch
        {
            { all: true } => (null, null),
            { year: null, day: null } when InAdvent => (Now.Year, Now.Day),
            { year: null, day: null } when !InAdvent && Now.Month == 12 => (Now.Year, null),
            { year: null, day: null } when !InAdvent => (null, null),
            { year: null, day: not null } when InAdvent => (Now.Year, day.Value),
            { year: null, day: not null } when !InAdvent => throw new ArgumentException("Outside the advent, it's meaningless to only specify a day"),
            { year: not null, day: not null } when !IsValidAndUnlocked(year.Value, day.Value) => throw new InvalidPuzzleException(new PuzzleKey(year.Value, day.Value)),
            _ => (year, day)
        };

        return from y in Years()
               where !year.HasValue || year.Value == y
               from d in Days(y)
               where !day.HasValue || day.Value == d
               select (y, d);
    }

    public bool IsValidAndUnlocked(int year, int day)
    {
        var now = Now;

        // no puzzles before 2015, nor in the future
        if (year < 2015) return false;
        if (year > now.Year) return false;

        // past years: day must be between 1 and 25
        if (year < now.Year) return day switch { >= 1 and <= 25 => true, _ => false };

        // current year: no puzzle's if we're not in december yet
        if (12 > now.Month) return false;

        // current year, december
        return day >= 1 && day <= now.Day && day <= 25;
    }

    public IEnumerable<int> Years()
    {
        for (int year = 2015; year <= Now.Year; year++)
            yield return year;
    }
    public IEnumerable<int> Days(int year)
    {
        var now = Now;
        for (int day = 1; (year < now.Year && day <= 25) ||
                          (now.Month == 12 && day <= Math.Min(25, now.Day)); day++)
            yield return day;
    }

    public bool IsToday(int y, int d)
    {
        if (y != Now.Year) return false;
        if (d != Now.Day) return false;
        return IsValidAndUnlocked(y, d);
    }

    internal void EnsureValid(PuzzleKey key)
    {
        if (!IsValidAndUnlocked(key.Year, key.Day))
            throw new InvalidPuzzleException(key);
    }
}

class InvalidPuzzleException : AoCException
{
    public InvalidPuzzleException(PuzzleKey key) : this($"Puzzle for {key} is invalid or not yet unlocked.")
    {
    }
    public InvalidPuzzleException()
    {
    }

    public InvalidPuzzleException(string? message) : base(message)
    {
    }

    public InvalidPuzzleException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}