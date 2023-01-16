
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System.Reflection;
using System.Threading.Tasks;

using Xunit;

namespace AoCTest.Year2017.Day03
{
    public class AoC
    {
        public long Part1() => 1;
        public long Part2() => 2;

    }
}

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{

    public class AoCRunnerTests
    {
        [Fact]
        public async Task Run_WithTypeName_Test()
        {
            var logger = Substitute.For<ILogger<AoCRunner>>();
            var resolver = Substitute.For<IAssemblyResolver>();
            var assembly = Assembly.GetExecutingAssembly();
            resolver.GetEntryAssembly().Returns(assembly);
            var runner = new AoCRunner(logger, resolver);
            var result = await runner.Run("AoCTest.Year{0}.Day{1:00}.AoC", 2017, 3, (i, s) => { });
            Assert.Equal("1", result.part1.Value);
            Assert.Equal("2", result.part2.Value);
        }
        [Fact]
        public async Task Run_WithoutTypeName_Test()
        {
            var logger = Substitute.For<ILogger<AoCRunner>>();
            var resolver = Substitute.For<IAssemblyResolver>();
            var assembly = Assembly.GetExecutingAssembly();
            resolver.GetEntryAssembly().Returns(assembly);
            var runner = new AoCRunner(logger, resolver);
            var result = await runner.Run(null, 2017, 3, (i, s) => { });
            Assert.Equal("1", result.part1.Value);
            Assert.Equal("2", result.part2.Value);

        }
    }

}