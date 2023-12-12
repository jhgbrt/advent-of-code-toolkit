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
        var filesystem = Substitute.For<IFileSystemFactory>();
        var m = new CodeManager(filesystem);

        var codeFolder = Substitute.For<ICodeFolder>();
        var templateFolder = Substitute.For<ITemplateFolder>();
        templateFolder.Notebook.Returns(new FileInfo("aoc.ipynb"));

        codeFolder.Exists.Returns(codeFolderExists);

        filesystem.GetCodeFolder(new(2021, 3)).Returns(codeFolder);
        filesystem.GetTemplateFolder().Returns(templateFolder);

        var code = @"
namespace AoC.Year2021.Day03;

public class AoC202103
{
    public AoC202103() : this(Read.InputLines())
    {
    }
    public AoC202103(string[] input)
    {
        myvariable = input.Select(int.Parse).ToArray();
    }
    int[] myvariable;
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
class Tests
{
    [Fact]
    public void Test1()
    {
        var sut = new AoC202103(new[] { ""1"", ""2"" });
        Assert.Equal(1, sut.Part1());
    }
}
";

        codeFolder.ReadCode().Returns(code);

        return m;
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderDoesNotExist_Succeeds()
    {
        CodeManager m = CreateCodeManager(false);
        var puzzle = Puzzle.Create(new(2021, 3), "input", Answer.Empty);
        await m.InitializeCodeAsync(puzzle, false, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_WithForce_Succeeds()
    {
        CodeManager m = CreateCodeManager(true);

        var puzzle = Puzzle.Create(new(2021, 3), "input", Answer.Empty);
        await m.InitializeCodeAsync(puzzle, true, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_Throws()
    {
        CodeManager m = CreateCodeManager(true);
        var puzzle = Puzzle.Create(new (2021, 3), "input", Answer.Empty);
        await Assert.ThrowsAsync<Exception>(async () => await m.InitializeCodeAsync(puzzle, false, s => { }));
    }

    [Fact]
    public async Task GenerateCodeTest()
    {
        var m = CreateCodeManager(true);

        var code = await m.GenerateCodeAsync(new(2021, 3));

        Assert.Equal(@"var input = File.ReadAllLines(""input.txt"");
int[] myvariable;
myvariable = input.Select(int.Parse).ToArray();
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
