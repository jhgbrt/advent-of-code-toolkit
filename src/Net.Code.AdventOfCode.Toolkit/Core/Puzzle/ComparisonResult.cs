namespace Net.Code.AdventOfCode.Toolkit.Core;

record ComparisonResult(ResultStatus part1, ResultStatus part2)
{
    public static implicit operator bool(ComparisonResult result) => result.part1 != ResultStatus.Ok || result.part2 != ResultStatus.Ok;
}
