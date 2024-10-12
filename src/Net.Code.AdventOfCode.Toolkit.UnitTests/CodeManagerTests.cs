using Net.Code.AdventOfCode.Toolkit.Core;

using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class CodeManagerTests
{
    const string code = """
        namespace AoC.Year2021.Day03;

        public class AoC202103
        {
            public AoC202103() : this(Read.InputLines(), Console.Out)
            {
            }
            public AoC202103(string[] input, TextWriter writer)
            {
                this.writer = writer;
                myvariable = input.Select(int.Parse).ToArray();
            }
            TextWriter writer;
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
        """;

    private static CodeManager CreateCodeManager(bool codeFolderExists, string code)
    {
        var filesystem = Substitute.For<IFileSystemFactory>();
        var m = new CodeManager(filesystem);

        var codeFolder = Substitute.For<ICodeFolder>();
        var templateFolder = Substitute.For<ITemplateFolder>();
        templateFolder.Notebook.Returns(new FileInfo("aoc.ipynb"));
        templateFolder.Exists.Returns(true);
        templateFolder.Sample.Returns(new FileInfo("sample.txt"));
        codeFolder.Exists.Returns(codeFolderExists);

        filesystem.GetCodeFolder(new(2021, 3)).Returns(codeFolder);
        filesystem.GetTemplateFolder(null).Returns(templateFolder);


        codeFolder.ReadCode().Returns(code);

        return m;
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderDoesNotExist_Succeeds()
    {
        CodeManager m = CreateCodeManager(false, code);
        var puzzle = Puzzle.Create(new(2021, 3), "input", Answer.Empty);
        await m.InitializeCodeAsync(puzzle, false, null, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_WithForce_Succeeds()
    {
        CodeManager m = CreateCodeManager(true, code);

        var puzzle = Puzzle.Create(new(2021, 3), "input", Answer.Empty);
        await m.InitializeCodeAsync(puzzle, true, null, s => { });
    }

    [Fact]
    public async Task InitializeCode_WhenCodeFolderExists_Throws()
    {
        CodeManager m = CreateCodeManager(true, code);
        var puzzle = Puzzle.Create(new (2021, 3), "input", Answer.Empty);
        await Assert.ThrowsAnyAsync<AoCException>(async () => await m.InitializeCodeAsync(puzzle, false, null, s => { }));
    }

    [Theory]
    [InlineData("""
        public class AoC202103
        {
            public object Part1() => 1;
            public object Part2() => 1;
        }
        """, """
        var sw = Stopwatch.StartNew();
        var part1 = 1;
        var part2 = 1;
        Console.WriteLine((part1, part2, sw.Elapsed));
        """)]
    [InlineData("""
        namespace AoC.Year2021.Day03;
    
        public class AoC202103
        {
            static string[] input = Read.InputLines();
            public object Part1() => 1;
            public object Part2() => 1;
        }
        """, """
        string[] input = File.ReadAllLines("input.txt");
        var sw = Stopwatch.StartNew();
        var part1 = 1;
        var part2 = 1;
        Console.WriteLine((part1, part2, sw.Elapsed));
        """)]
    [InlineData("""
        namespace AoC.Year2021.Day03;
    
        public class AoC202103
        {
            public AoC202103() : this(Read.InputLines()) {}
            const int A = 42;
            Grid grid;
            public AoC202103(string[] input)
            {
                grid = new Grid(input);
            }
            public object Part1() => 1;
            public object Part2() => 1;
        }
        """, """
        int A = 42;
        var input = File.ReadAllLines("input.txt");
        var grid = new Grid(input);
        var sw = Stopwatch.StartNew();
        var part1 = 1;
        var part2 = 1;
        Console.WriteLine((part1, part2, sw.Elapsed));
        """)]
    [InlineData("""
        namespace AoC.Year2021.Day03;
    
        public class AoC202103
        {
            public AoC202103() : this(Read.InputLines(), Console.Out) {}
            Grid grid;
            TextWriter writer;
            public AoC202103(string[] input, TextWriter writer)
            {
                this.writer = writer;
                grid = new Grid(input);
            }
            public object Part1() => 1;
            public object Part2() => 1;
        }
        """, """
        var input = File.ReadAllLines("input.txt");
        var writer = Console.Out;
        var grid = new Grid(input);
        var sw = Stopwatch.StartNew();
        var part1 = 1;
        var part2 = 1;
        Console.WriteLine((part1, part2, sw.Elapsed));
        """)]
    [InlineData("""
        namespace AoC.Year2021.Day03;
    
        public class AoC202103
        {
            public object Part1() => Solve(1);
            public object Part2() => Solve(2);
            private object Solve(int i) => i;
        }
        """, """
        var sw = Stopwatch.StartNew();
        var part1 = Solve(1);
        var part2 = Solve(2);
        Console.WriteLine((part1, part2, sw.Elapsed));
        object Solve(int i) => i;
        """)]
    [InlineData("""
        public class AoC202103
        {
            string[] input;
            public AoC202103() 
            {
                input = Read.InputLines();
            }
            public object Part1() => 1;
            public object Part2() => 2;
        }
        """, """
        var input = File.ReadAllLines("input.txt");
        var sw = Stopwatch.StartNew();
        var part1 = 1;
        var part2 = 2;
        Console.WriteLine((part1, part2, sw.Elapsed));
        """)]
        [InlineData("""
        public class AoC202103
        {
            public AoC202318():this(Read.InputLines(), Console.Out) {}
            readonly TextWriter writer;
            string[] input;
            readonly ImmutableArray<string> items;
            public AoC202318(string[] input, TextWriter writer)
            {
                this.input = input;
                items = input.Select(s =>
                    {
                        return s;
                    }
                ) .ToImmutableArray();
                this.writer = writer;
            }
            public object Part1() => 1;
            public object Part2() => 2;
        }
        """, """
        var input = File.ReadAllLines("input.txt");
        var writer = Console.Out;
        var items = input.Select(s =>
        {
            return s;
        }).ToImmutableArray();
        var sw = Stopwatch.StartNew();
        var part1 = 1;
        var part2 = 2;
        Console.WriteLine((part1, part2, sw.Elapsed));
        """)]
    public async Task GenerateCodeTests(string input, string expected)
    {
        var m = CreateCodeManager(true, input);

        var code = await m.GenerateCodeAsync(new(2021, 3));

        Assert.Equal(expected, code);
    }

    [Fact]
    public async Task GenerateCodeTest()
    {
        var m = CreateCodeManager(true, code);

        var result = await m.GenerateCodeAsync(new(2021, 3));

        Assert.Equal("""
            var input = File.ReadAllLines("input.txt");
            var writer = Console.Out;
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
