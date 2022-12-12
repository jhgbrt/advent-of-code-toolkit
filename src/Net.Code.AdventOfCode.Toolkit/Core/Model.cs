namespace Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using System.ComponentModel.DataAnnotations;

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
record DayResult(int year, int day, Result part1, Result part2)
{
    public readonly static DayResult Empty = new DayResult(0, 0, Result.Empty, Result.Empty);
    public static DayResult NotImplemented(int year, int day) => new DayResult(year, day, Result.Empty, Result.Empty);
    public TimeSpan Elapsed => part1.Elapsed + part2.Elapsed;
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

record Puzzle(int Year, int Day, string Html, string Text, string Input, Answer Answer, Status Status)
{
    public int Unanswered => Status switch { Status.Completed => 0, Status.AnsweredPart1 => 1, _ => 2 };
    public static Puzzle Locked(int year, int day) => new(year, day, string.Empty, string.Empty, string.Empty, Answer.Empty, Status.Locked);
    public static Puzzle Unlocked(int year, int day, string html, string text, string input, Answer answer) => new(year, day, html, text, input, answer, answer switch
    {
        { part1: "", part2: "" } => Status.Unlocked,
        { part1: not "", part2: "" } => day < 25 ? Status.AnsweredPart1 : Status.Completed,
        { part1: not "", part2: not "" } => Status.Completed,
        _ => throw new Exception($"inconsistent state for {year}/{day}/{answer}")
    });

    public ComparisonResult Compare(DayResult result)
    {
        if ((result.year, result.day) != (Year, Day)) throw new InvalidOperationException("Result is for another day");

        return Day switch
        {
            25 => new ComparisonResult(result.part1.Verify(Answer.part1).Status, ResultStatus.Ok),
            _ => new ComparisonResult(result.part1.Verify(Answer.part1).Status, result.part2.Verify(Answer.part2).Status)
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
    public bool Ok => result.part1.Value == puzzle.Answer.part1 && result.part2.Value == puzzle.Answer.part2;

    public string ToReportLine()
    {
        var duration = result.Elapsed.ToString();
        var comparisonResult = puzzle.Compare(result);
        var status1 = GetReportPart(comparisonResult.part1, puzzle.Answer.part1, result.part1.Value, 1);
        var status2 = GetReportPart(comparisonResult.part2, puzzle.Answer.part2, result.part2.Value, 2);
        return $"{result.year}-{result.day:00} {status1.status}/{status2.status} - {duration} - {status1.explanation} {status2.explanation}";
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
        var status1 = GetReportPart(comparisonResult.part1, puzzle.Answer.part1, result.part1.Value, 1);
        var status2 = GetReportPart(comparisonResult.part2, puzzle.Answer.part2, result.part2.Value, 2);
        return $"{result.year}-{result.day:00} [[[{status1.color}]{status1.status}[/]]]/[[[{status2.color}]{status2.status}[/]]] [{dcolor}]{duration}[/]{status1.explanation} {status2.explanation}";
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