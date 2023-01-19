namespace Net.Code.AdventOfCode.Toolkit.Core;

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
