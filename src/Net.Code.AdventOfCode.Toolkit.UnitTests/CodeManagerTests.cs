using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System;
using System.Threading.Tasks;

using Xunit;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class CodeManagerTests
{
    private static CodeManager CreateCodeManager(bool codeFolderExists)
    {
        var filesystem = Substitute.For<IFileSystem>();
        var m = new CodeManager(filesystem);

        var codeFolder = Substitute.For<ICodeFolder>();
        var templateFolder = Substitute.For<ITemplateFolder>();
        templateFolder.Notebook.Returns(new FileInfo("aoc.ipynb"));

        codeFolder.Exists.Returns(codeFolderExists);

        filesystem.GetCodeFolder(2021, 3).Returns(codeFolder);
        filesystem.GetTemplateFolder().Returns(templateFolder);

        var code = @"
namespace AoC.Year2021.Day03;

public class AoC
{
    static string[] input = Read.InputLines();
    static int[] myvariable = input.Select(int.Parse).ToArray();
    public object Part1() => Solve(1);
    public object Part2()
    {
        return Solve(2);
    }
    public long Solve(int part)
    {
        return ToLong(part);
    }
    static long ToLong(int i) => i;
}

record MyRecord();
class MyClass
{
}
";

        codeFolder.ReadCode().Returns(code);

        return m;
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderDoesNotExist_Succeeds()
    {
        CodeManager m = CreateCodeManager(false);
        var puzzle = Puzzle.Unlocked(2021, 3, "input", Answer.Empty);
        await m.InitializeCodeAsync(puzzle, false, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_WithForce_Succeeds()
    {
        CodeManager m = CreateCodeManager(true);

        var puzzle = Puzzle.Unlocked(2021, 3, "input", Answer.Empty);
        await m.InitializeCodeAsync(puzzle, true, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_Throws()
    {
        CodeManager m = CreateCodeManager(true);
        var puzzle = Puzzle.Unlocked(2021, 3, "input", Answer.Empty);
        await Assert.ThrowsAsync<Exception>(async () => await m.InitializeCodeAsync(puzzle, false, s => { }));
    }

    [Fact]
    public async Task GenerateCodeTest()
    {
        var m = CreateCodeManager(true);

        var code = await m.GenerateCodeAsync(2021, 3);

        Assert.Equal(@"var input = File.ReadAllLines(""input.txt"");
var myvariable = input.Select(int.Parse).ToArray();
var sw = Stopwatch.StartNew();
var part1 = Solve(1);
var part2 = Part2();
Console.WriteLine((part1, part2, sw.Elapsed));
object Part2()
{
    return Solve(2);
}

long Solve(int part)
{
    return ToLong(part);
}

long ToLong(int i) => i;
record MyRecord();
class MyClass
{
}", code, ignoreLineEndingDifferences: true);

    }
}
