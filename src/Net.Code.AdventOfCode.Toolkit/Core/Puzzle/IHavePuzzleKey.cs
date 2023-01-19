namespace Net.Code.AdventOfCode.Toolkit.Core;

interface IHavePuzzleKey
{
    PuzzleKey Key { get; }
    int Year { get; }
    int Day { get; }
}
