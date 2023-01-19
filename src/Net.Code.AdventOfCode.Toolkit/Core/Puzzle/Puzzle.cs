namespace Net.Code.AdventOfCode.Toolkit.Core;

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
    private Puzzle()
    {
        Input = string.Empty;
        Answer = Answer.Empty;
    }

    public static Puzzle Locked(PuzzleKey key) => new(key, string.Empty, Answer.Empty, Status.Locked);
    public static Puzzle Create(PuzzleKey key, string input, Answer answer) => new(key, input, answer, answer switch
    {
        { part1: "", part2: "" } => Status.Unlocked,
        { part1: not "", part2: "" } => key.Day < 25 ? Status.AnsweredPart1 : Status.Completed,
        { part1: not "", part2: not "" } => Status.Completed,
        _ => throw new Exception($"inconsistent state for {key}/{answer}")
    });

    public AnswerToPost CreateAnswer(string answer) => Status switch
    {
        Status.Locked => throw new Exception("Puzzle is locked. Did you initialize it?"),
        Status.Completed => throw new Exception("Already completed"),
        Status.Unlocked => new(1, answer),
        Status.AnsweredPart1 => new(2, answer),
        _ => throw new NotSupportedException()
    };

    public void SetAnswer(AnswerToPost answer)
    {
        Answer = answer.part switch
        {
            1 => Answer with { part1 = answer.value },
            2 => Answer with { part2 = answer.value },
            _ => throw new NotSupportedException()
        };
    }
}
