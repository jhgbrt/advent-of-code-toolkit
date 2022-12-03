using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System.Threading.Tasks;

using Xunit;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests;

public class CacheTests
{
    [Theory]
    [InlineData(null, null, "c:", "base", ".cache", "name.txt")]
    [InlineData(2020, null, "c:", "base", ".cache", "2020", "name.txt")]
    [InlineData(2020, 1, "c:", "base", ".cache", "2020", "01", "name.txt")]
    public async Task ReadFromCacheTest(int? year, int? day, params string[] path)
    {
        var fullpath = Path.Combine(path);
        ILogger<Cache> logger = Substitute.For<ILogger<Cache>>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.CurrentDirectory.Returns(Path.Combine("c:", "base"));
        fileSystem.ReadAllTextAsync(fullpath).Returns(Task.FromResult("content"));
        var cache = new Cache(logger, fileSystem);
        var result = await cache.ReadFromCache(year, day, "name.txt");
        Assert.Equal("content", result);
    }
    [Theory]
    [InlineData(null, null, "c:", "base", ".cache", "name.txt")]
    [InlineData(2020, null, "c:", "base", ".cache", "2020", "name.txt")]
    [InlineData(2020, 1, "c:", "base", ".cache", "2020", "01", "name.txt")]
    public async Task WriteToCacheTest(int? year, int? day, params string[] path)
    {
        var fullpath = Path.Combine(path);
        ILogger<Cache> logger = Substitute.For<ILogger<Cache>>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.CurrentDirectory.Returns(Path.Combine("c:", "base"));
        fileSystem.WriteAllTextAsync(fullpath, "content").Returns(Task.FromResult(0));
        var cache = new Cache(logger, fileSystem);
        await cache.WriteToCache(year, day, "name.txt", "content");
        await fileSystem.Received(1).WriteAllTextAsync(fullpath, "content");
    }


    [Theory]
    [InlineData(null, null, "c:", "base", ".cache", "name.txt")]
    [InlineData(2020, null, "c:", "base", ".cache", "2020", "name.txt")]
    [InlineData(2020, 1, "c:", "base", ".cache", "2020", "01", "name.txt")]
    public void ExistsTest(int? year, int? day, params string[] path)
    {
        var fullpath = Path.Combine(path);
        ILogger<Cache> logger = Substitute.For<ILogger<Cache>>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.CurrentDirectory.Returns(Path.Combine("c:", "base"));
        fileSystem.FileExists(fullpath).Returns(true);
        var cache = new Cache(logger, fileSystem);
        Assert.True(cache.Exists(year, day, "name.txt"));
        Assert.False(cache.Exists(year, day, "blah.txt"));
    }

}