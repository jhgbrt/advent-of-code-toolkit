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
    [InlineData(@"C:\base\.cache\name.txt", null, null)]
    [InlineData(@"C:\base\.cache\2020\name.txt", 2020, null)]
    [InlineData(@"C:\base\.cache\2020\01\name.txt", 2020, 1)]
    public async Task ReadFromCacheTest(string fullpath, int? year, int? day)
    {
        ILogger<Cache> logger = Substitute.For<ILogger<Cache>>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.CurrentDirectory.Returns(@"C:\base");
        fileSystem.ReadAllTextAsync(fullpath).Returns(Task.FromResult("content"));
        var cache = new Cache(logger, fileSystem);
        var result = await cache.ReadFromCache(year, day, "name.txt");
        Assert.Equal("content", result);
    }
    [Theory]
    [InlineData(@"C:\base\.cache\name.txt", null, null)]
    [InlineData(@"C:\base\.cache\2020\name.txt", 2020, null)]
    [InlineData(@"C:\base\.cache\2020\01\name.txt", 2020, 1)]
    public async Task WriteToCacheTest(string fullpath, int? year, int? day)
    {
        ILogger<Cache> logger = Substitute.For<ILogger<Cache>>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.CurrentDirectory.Returns(@"C:\base");
        fileSystem.WriteAllTextAsync(fullpath, "content").Returns(Task.FromResult(0));
        var cache = new Cache(logger, fileSystem);
        await cache.WriteToCache(year, day, "name.txt", "content");
        await fileSystem.Received(1).WriteAllTextAsync(fullpath, "content");
    }


    [Theory]
    [InlineData(@"C:\base\.cache\name.txt", null, null)]
    [InlineData(@"C:\base\.cache\2020\name.txt", 2020, null)]
    [InlineData(@"C:\base\.cache\2020\01\name.txt", 2020, 1)]
    public void ExistsTest(string fullpath, int? year, int? day)
    {
        ILogger<Cache> logger = Substitute.For<ILogger<Cache>>();
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.CurrentDirectory.Returns(@"C:\base");
        fileSystem.FileExists(fullpath).Returns(true);
        var cache = new Cache(logger, fileSystem);
        Assert.True(cache.Exists(year, day, "name.txt"));
        Assert.False(cache.Exists(year, day, "blah.txt"));
    }

}