namespace Net.Code.AdventOfCode.Toolkit.Core;

record PuzzleResultStatus(Puzzle puzzle, DayResult result)
{
    public bool Ok => result.Part1.Value == puzzle.Answer.part1 && result.Part2.Value == puzzle.Answer.part2;

    public string ToReportLine()
    {
        var duration = result.Elapsed.ToString();
        var comparisonResult = Comparison;
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

        var comparisonResult = Comparison;
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

    public ComparisonResult Comparison
    {
        get
        {
            if (result.Key != puzzle.Key) throw new InvalidOperationException("Result is for another day");

            return puzzle.Day switch
            {
                25 => new ComparisonResult(result.Part1.Verify(puzzle.Answer.part1).Status, ResultStatus.Ok),
                _ => new ComparisonResult(result.Part1.Verify(puzzle.Answer.part1).Status, result.Part2.Verify(puzzle.Answer.part2).Status)
            };
        }
    }
}
