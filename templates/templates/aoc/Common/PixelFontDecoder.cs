namespace AdventOfCode;


static class PixelFontDecoder
{
    static readonly string[] _4x6 = new[]
        {
            ".##.#..##..######..##..#",
            "###.#..####.#..##..####.",
            ".####...#...#...#....###",
            "###.#..##..##..##..####.",
            "#####...###.#...#...####",
            "#####...###.#...#...#...",
            ".####...#...#.###..#.##.",
            "#..##..######..##..##..#",
            "###..#...#...#...#..###.",
            "..##...#...#...##..#.##.",
            "#..##.#.##..##..#.#.#..#",
            "#...#...#...#...#...####",
            "#..######..##..##..##..#",
            "#..###.###.##.###.###..#",
            ".##.#..##..##..##..#.##.",
            "###.#..##..####.#...#...",
            ".##.#..##..##..##.##.###",
            "###.#..##..####.#.#.#..#",
            ".####...#....##....####.",
            "####.#...#...#...#...#..",
            "#..##..##..##..##..#.##.",
            "#..##..##..#.##..##..##.",
            "#..##..##..##..######..#",
            "#..##..#.##.#..##..##..#",
            "#..##..#.##...#...#..#..",
            "####...#..#..#..#...####"
        };

    static readonly string[] _6x10 = new[]
    {
        "..##...#..#.#....##....##....########....##....##....##....#",
        "#####.#....##....##....######.#....##....##....##....######.",
        ".####.#....##.....#.....#.....#.....#.....#.....#....#.####.",
        "#####.#....##....##....##....##....##....##....##....######.",
        "#######.....#.....#.....#####.#.....#.....#.....#.....######",
        "#######.....#.....#.....#####.#.....#.....#.....#.....#.....",
        ".######.....#.....#.....#.....#..####....##....##....#.####.",
        "#....##....##....##....########....##....##....##....##....#",
        "#####...#.....#.....#.....#.....#.....#.....#.....#...#####.",
        "...###....#.....#.....#.....#.....#.....#.#...#.#...#..###..",
        "#....##...#.#..#..#.#...##....##....#.#...#..#..#...#.#....#",
        "#.....#.....#.....#.....#.....#.....#.....#.....#.....######",
        "#....###..###.##.##....##....##....##....##....##....##....#",
        "#....##....###...###...##.#..##..#.##...###...###....##....#",
        ".####.#....##....##....##....##....##....##....##....#.####.",
        "#####.#....##....##....######.#.....#.....#.....#.....#.....",
        ".####.#....##....##....##....##....##....##....##...#..###.#",
        "#####.#....##....##....######.#..#..#...#.#...#.#....##....#",
        ".####.#.....#.....#......####......#.....#.....#.....######.",
        "######..#.....#.....#.....#.....#.....#.....#.....#.....#...",
        "#....##....##....##....##....##....##....##....##....#.####.",
        "#....##....##....##....#.#..#..#..#..#. #..#..#...##....##..",
        "#....##....##....##....##....##....##....##.##.###..###....#",
        "#....##....#.#..#..#..#...##....##...#..#..#..#.#....##....#",
        "#....##....#.#..#..#..#...##.....#.....#.....#.....#....#...",
        "######.....#.....#....#....#....#....#....#.....#.....######"

    };
    static IReadOnlyDictionary<int, (string s, char c)[]> lettersBySize = new[]
    {
        (size: 5, letters: _4x6.Select((s, i) => (s, (char)(i + 'A'))).ToArray()),
        (size: 7, letters: _6x10.Select((s, i) => (s, (char)(i + 'A'))).ToArray())
    }.ToDictionary(x => x.size, x => x.letters);

    // use this function to generate a simple string representing the character
    public static IEnumerable<(string, char)> FlattenLetters(string s, int size, char pixel = '#', char blank = '.')
        => from letter in FindLetters(s, size)
           let flattenedValue = letter.Aggregate(new StringBuilder(), (sb, range) => sb.Append(s[range]).Replace(pixel, '#').Replace(blank, '.')).ToString()
           let result = lettersBySize[size].Where(l => l.s == flattenedValue).Select(l => (char?)l.c).FirstOrDefault() ?? '?'
           select (flattenedValue, result);

    public static string DecodePixels(this string s, int size, char pixel = '#', char blank = '.') => (
            from letter in FindLetters(s, size)
            let chars = from range in letter from c in s[range] select c switch { '#' => pixel, _ => blank }
            select (from item in lettersBySize[size] where item.s.SequenceEqual(chars) select (char?)item.c).SingleOrDefault() ?? '?'
        ).Aggregate(new StringBuilder(), (sb, c) => sb.Append(c)).ToString();

    private static IEnumerable<IGrouping<int, Range>> FindLetters(string s, int size)
        => from slice in s.Lines()
           from item in slice.Chunk(size).Select((c, i) => (c: new Range(c.Start, c.End.Value - 1), i))
           let chunk = item.c
           let index = item.i
           group chunk by index;

    private static IEnumerable<Range> Lines(this string s)
    {
        int x = 0;
        while (x < s.Length)
        {
            var newline = s.IndexOf('\n', x);
            if (newline == -1) break;
            var count = newline switch { > 0 when s[newline - 1] == '\r' => newline - x - 1, _ => newline - x };
            yield return new(x, x + count);
            x = newline + 1;
        }
    }
    private static IEnumerable<Range> Chunk(this Range range, int size)
    {
        int s = range.Start.Value;
        while (s < range.End.Value)
        {
            yield return new Range(s, s + (size > range.End.Value ? range.End.Value - s : size));
            s += size;
        }
    }

}

public class PixelFontDecoderTests
{
    [Fact]
    public void Test6x10()
    {
        var s = @"
..##...#####...####.....###.#####..#....#.######
.#..#..#....#.#....#.....#..#....#.#....#......#
#....#.#....#.#..........#..#....#..#..#.......#
#....#.#....#.#..........#..#....#..#..#......#.
#....#.#####..#..........#..#####....##......#..
######.#....#.#..........#..#..#.....##.....#...
#....#.#....#.#..........#..#...#...#..#...#....
#....#.#....#.#......#...#..#...#...#..#..#.....
#....#.#....#.#....#.#...#..#....#.#....#.#.....
#....#.#####...####...###...#....#.#....#.######
".Remove(0, Environment.NewLine.Length);
        Assert.Equal("ABCJRXZ", s.DecodePixels(7));
    }
    [Fact]
    public void Test4x6()
    {
        var s = @"
####.###..#..#.####.#....###..###..###.
#....#..#.#..#.#....#....#..#.#..#.#..#
###..#..#.#..#.###..#....#..#.###..#..#
#....###..#..#.#....#....###..#..#.###.
#....#....#..#.#....#....#....#..#.#.#.
####.#.....##..####.####.#....###..#..#
".Remove(0, Environment.NewLine.Length);

        Assert.Equal("EPUELPBR", s.DecodePixels(5));
    }
}