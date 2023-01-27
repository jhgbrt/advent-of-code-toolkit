namespace AdventOfCode.YearYYYY.DayDD;
public class AoCYYYYDD
{
    static bool usesample = true;
    static string[] sample = Read.SampleLines();
    static string[] realinput = Read.InputLines();
    static string[] input = usesample ? sample : realinput;
    static ImmutableArray<Item> items = input.Select(s => Regexes.MyRegex().As<Item>(s)).ToImmutableArray();
    public object Part1()
    {
        Console.WriteLine(string.Join(Environment.NewLine, input));

        foreach (var item in items)
            Console.WriteLine(item);

        return -1;
    }
    public object Part2() => "";
}

readonly record struct Item(string name, int n);

static partial class Regexes
{
    [GeneratedRegex(@"^(?<name>.*): (?<n>\d+)$")]
    public static partial Regex MyRegex();
}