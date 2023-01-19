namespace Net.Code.AdventOfCode.Toolkit.Core;

record Answer(string part1, string part2)
{
    public static Answer Empty => new Answer(string.Empty, string.Empty);
}

record AnswerToPost(int part, string value);
