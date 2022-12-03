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
        var client = Substitute.For<IAoCClient>();
        var filesystem = Substitute.For<IFileSystem>();
        var m = new CodeManager(client, filesystem);

        var codeFolder = Substitute.For<ICodeFolder>();
        var templateFolder = Substitute.For<ITemplateFolder>();

        codeFolder.Exists.Returns(codeFolderExists);
        templateFolder.Exists.Returns(true);

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

        await m.InitializeCodeAsync(2021, 3, false, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_WithForce_Succeeds()
    {
        CodeManager m = CreateCodeManager(true);

        await m.InitializeCodeAsync(2021, 3, true, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_Trhows()
    {
        CodeManager m = CreateCodeManager(true);

        await Assert.ThrowsAsync<Exception>(async () => await m.InitializeCodeAsync(2021, 3, false, s => { }));
    }

    [Fact]
    public async Task GenerateCodeTest()
    {
        var m = CreateCodeManager(true);

        var code = await m.GenerateCodeAsync(2021, 3);

        Assert.Equal(@"var input = File.ReadAllLines(""input.txt"");
var myvariable = input.Select(int.Parse).ToArray();
var sw = Stopwatch.StartNew();
var part1 = Part1();
var part2 = Part2();
Console.WriteLine((part1, part2, sw.Elapsed));
object Part1() => Solve(1);
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
