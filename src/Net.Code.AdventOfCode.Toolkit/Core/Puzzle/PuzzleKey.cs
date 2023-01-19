namespace Net.Code.AdventOfCode.Toolkit.Core;

record struct PuzzleKey(int Year, int Day)
{
    public PuzzleKey(int id) : this(id / 100, id % 100) { }
    public int Id => Year * 100 + Day;
    public override string ToString() => $"{Year}-{Day:00}";
}
