using Net.Code.AdventOfCode.Toolkit.Core;

using Net.Code.AdventOfCode.Toolkit.Logic;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class CodeManagerTests
{
    IFileSystem filesystem;
    CodeManager codemanager;
    public CodeManagerTests()
    {
        filesystem = Mocks.FileSystem();
        codemanager = new CodeManager(filesystem);
    }
    const string code = """
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
                        """;

    [Fact]
    public async Task InitializeCode_WhenCodeFolderDoesNotExist_Succeeds()
    {
        var puzzle = Puzzle.Create(new(2021, 3), "input", Answer.Empty);
        await codemanager.InitializeCodeAsync(puzzle, false, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_WithForce_Succeeds()
    {
        var codefolder = new DirectoryInfo(@"c:\aoc\Year2021\Day03");
        filesystem.Create(codefolder);
        await filesystem.Write(codefolder.GetCodeFile(), code);
        var puzzle = Puzzle.Create(new(2021, 3), "input", Answer.Empty);
        await codemanager.InitializeCodeAsync(puzzle, true, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_Throws()
    {
        var codefolder = new DirectoryInfo(@"c:\aoc\Year2021\Day03");
        filesystem.Create(codefolder);
        await filesystem.Write(codefolder.GetCodeFile(), code);
        var puzzle = Puzzle.Create(new (2021, 3), "input", Answer.Empty);
        await Assert.ThrowsAsync<Exception>(async () => await codemanager.InitializeCodeAsync(puzzle, false, s => { }));
    }

    [Fact]
    public async Task GenerateCodeTest()
    {
        var codefolder = new DirectoryInfo(@"c:\aoc\Year2021\Day03");
        filesystem.Create(codefolder);
        await filesystem.Write(codefolder.GetCodeFile(), code);
        var result = await codemanager.GenerateCodeAsync(new(2021, 3));

        Assert.Equal("""
                     var input = File.ReadAllLines("input.txt");
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
                     }
                     """, result, ignoreLineEndingDifferences: true);

    }
}
