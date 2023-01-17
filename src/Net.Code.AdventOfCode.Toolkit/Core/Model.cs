namespace Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

enum ResultStatus
{
    [Display(Name = "N/A")]
    NotImplemented, // not implemented
    Unknown,        // unknown if correct or not
    Failed,         // failed after verification
    Ok,              // correct after verification
    AnsweredButNotImplemented
}
record ComparisonResult(ResultStatus part1, ResultStatus part2)
{
    public static implicit operator bool(ComparisonResult result) => result.part1 != ResultStatus.Ok || result.part2 != ResultStatus.Ok;
}

record Answer(string part1, string part2)
{
    public static Answer Empty => new Answer(string.Empty, string.Empty);
}
record DayResultV1(int year, int day, Result part1, Result part2);

record struct PuzzleKey(int Year, int Day)
{
    public PuzzleKey(int id) : this(id / 100, id % 100) { }
    public int Id => Year*100 + Day;
}
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

    public DayResult(int year, int day, Result part1, Result part2)
        : this(new PuzzleKey(year, day), part1, part2)
    {
    }

    private DayResult()
    {
        Part1 = Result.Empty;
        Part2 = Result.Empty;
    }

    public readonly static DayResult Empty = new DayResult(0, 0, Result.Empty, Result.Empty);
    public static DayResult NotImplemented(int year, int day) => new DayResult(year, day, Result.Empty, Result.Empty);
    public TimeSpan Elapsed { get; init; }

    public override string ToString()
    {
        return $"{Key} (year {Year}, day {Day}) - Elapsed = {Elapsed}; Part1 = {Part1}, Part2 = {Part2}";
    }
}

record Result(ResultStatus Status, string Value, TimeSpan Elapsed)
{
    public readonly static Result Empty = new Result(ResultStatus.NotImplemented, string.Empty, TimeSpan.Zero);
    public Result Verify(string answer) => Status switch
    {
        ResultStatus.Unknown when string.IsNullOrEmpty(answer) => this,
        ResultStatus.Unknown => this with { Status = answer == Value ? ResultStatus.Ok : ResultStatus.Failed },
        ResultStatus.NotImplemented when !string.IsNullOrEmpty(answer) => this with { Status = ResultStatus.AnsweredButNotImplemented },
        _ => this
    };
}
interface IHavePuzzleKey
{
    PuzzleKey Key { get; }
    int Year { get; }
    int Day { get; }
}
class Puzzle : IHavePuzzleKey
{
    public PuzzleKey Key { get; init; }
    public int Year { get; init; }
    public int Day { get; init; }
    public string Input { get; init; }
    public Answer Answer { get; set; }
    public Status Status { get; set; }

    public Puzzle(PuzzleKey key, string input, Answer answer, Status status)
    {
        Key = key;
        Year = key.Year;
        Day = key.Day;
        Input = input;
        Answer = answer;
        Status = status;
    }
    public Puzzle(int year, int day, string input, Answer answer, Status status)
        : this (new PuzzleKey(year, day), input, answer, status)
    {
    }
    private Puzzle()
    {
        Input = string.Empty;
        Answer = Answer.Empty;
    }

    public int Unanswered => Status switch { Status.Completed => 0, Status.AnsweredPart1 => 1, _ => 2 };
    public static Puzzle Locked(int year, int day) => new(year, day, string.Empty, Answer.Empty, Status.Locked);
    public static Puzzle Unlocked(int year, int day, string input, Answer answer) => new(year, day, input, answer, answer switch
    {
        { part1: "", part2: "" } => Status.Unlocked,
        { part1: not "", part2: "" } => day < 25 ? Status.AnsweredPart1 : Status.Completed,
        { part1: not "", part2: not "" } => Status.Completed,
        _ => throw new Exception($"inconsistent state for {year}/{day}/{answer}")
    });

    public ComparisonResult Compare(DayResult result)
    {
        if ((result.Year, result.Day) != (Year, Day)) throw new InvalidOperationException("Result is for another day");

        return Day switch
        {
            25 => new ComparisonResult(result.Part1.Verify(Answer.part1).Status, ResultStatus.Ok),
            _ => new ComparisonResult(result.Part1.Verify(Answer.part1).Status, result.Part2.Verify(Answer.part2).Status)
        };
    }

}
public enum Status
{
    Locked,
    Unlocked,
    AnsweredPart1,
    Completed
}

record LeaderBoard(int OwnerId, int Year, IReadOnlyDictionary<int, Member> Members);
record Member(int Id, string Name, int TotalStars, int LocalScore, int GlobalScore, Instant? LastStarTimeStamp, IReadOnlyDictionary<int, DailyStars> Stars);
record DailyStars(int Day, Instant? FirstStar, Instant? SecondStar);

record PuzzleResultStatus(Puzzle puzzle, DayResult result)
{
    public bool Ok => result.Part1.Value == puzzle.Answer.part1 && result.Part2.Value == puzzle.Answer.part2;

    public string ToReportLine()
    {
        var duration = result.Elapsed.ToString();
        var comparisonResult = puzzle.Compare(result);
        var status1 = GetReportPart(comparisonResult.part1, puzzle.Answer.part1, result.Part1.Value, 1);
        var status2 = GetReportPart(comparisonResult.part2, puzzle.Answer.part2, result.Part2.Value, 2);
        return $"{result.Year}-{result.Day:00} {status1.status}/{status2.status} - {duration} - {status1.explanation} {status2.explanation}";
    }

    public string ToReportLineMarkup()
    {
        (var duration, var dcolor) = result.Elapsed.TotalMilliseconds switch
        {
            < 10 => ("< 10 ms", Console.ForegroundColor),
            < 100 => ("< 100 ms", Console.ForegroundColor),
            < 1000 => ("< 1s", Console.ForegroundColor),
            double value when value < 3000 => ($"~ {(int)Math.Round(value / 1000)} s", ConsoleColor.Yellow),
            double value => ($"~ {(int)Math.Round(value / 1000)} s", ConsoleColor.Red)
        };

        var comparisonResult = puzzle.Compare(result);
        var status1 = GetReportPart(comparisonResult.part1, puzzle.Answer.part1, result.Part1.Value, 1);
        var status2 = GetReportPart(comparisonResult.part2, puzzle.Answer.part2, result.Part2.Value, 2);
        return $"{result.Year}-{result.Day:00} [[[{status1.color}]{status1.status}[/]]]/[[[{status2.color}]{status2.status}[/]]] [{dcolor}]{duration}[/]{status1.explanation} {status2.explanation}";
    }
    (string status, ConsoleColor color, string explanation) GetReportPart(ResultStatus status, string answer, string result, int part) => status switch
    {
        ResultStatus.Failed => ("FAILED", ConsoleColor.Red, $" - part {part}: expected {answer} but was {result}"),
        ResultStatus.AnsweredButNotImplemented => ("ERROR", ConsoleColor.Red, $" - part {part} answered, but no implementation found"),
        ResultStatus.NotImplemented => ("NOTIMPLEMENTED", ConsoleColor.Yellow, ""),
        ResultStatus.Unknown => ("UNKNOWN", ConsoleColor.Yellow, $" - post answer for part {part} to verify"),
        ResultStatus.Ok => ("OK", ConsoleColor.Green, ""),
        _ => throw new NotImplementedException($"{status} is an unhandled result status")
    };
}


record LeaderboardEntry(string name, int year, long score, long stars, DateTimeOffset lastStar);
record PuzzleReportEntry(
    int year, int day, string answer1, string answer2,
    string result1, TimeSpan elapsed1, ResultStatus status1,
    string result2, TimeSpan elapsed2, ResultStatus status2,
    TimeSpan elapsedTotal);

record MemberStats(string name, int stars, int score);