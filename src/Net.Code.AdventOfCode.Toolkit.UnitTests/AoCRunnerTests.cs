
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System.Reflection;


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
            var result = await runner.Run("AoCTest.Year{0}.Day{1:00}.AoC{0}{1:00}", new(2017, 3), (i, s) => { });
            Assert.Equal("answer1", result!.Part1.Value);
            Assert.Equal("answer2", result.Part2.Value);
        }
        [Fact]
        public async Task Run_WithoutTypeName_Test()
        {
            var logger = Substitute.For<ILogger<AoCRunner>>();
            var resolver = Substitute.For<IAssemblyResolver>();
            var assembly = Assembly.GetExecutingAssembly();
            resolver.GetEntryAssembly().Returns(assembly);
            var runner = new AoCRunner(logger, resolver);
            var result = await runner.Run(null, new(2017, 3), (i, s) => { });
            Assert.Equal("answer1", result!.Part1.Value);
            Assert.Equal("answer2", result.Part2.Value);

        }
    }

}