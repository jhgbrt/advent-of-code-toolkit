
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class Cache : ICache
{
    ILogger<Cache> logger;
    private readonly IFileSystem fileSystem;

    public Cache(ILogger<Cache> logger, IFileSystem fileSystem)
    {
        this.logger = logger;
        this.fileSystem = fileSystem;
        fileSystem.CreateDirectoryIfNotExists(BaseDir, FileAttributes.Hidden);
    }

    private string BaseDir => Path.Combine(fileSystem.CurrentDirectory, ".cache");
    private string GetDirectory(int? year, int? day)
    {
        var path = (year, day) switch
        {
            (null, _) => BaseDir,
            (not null, null) => Path.Combine(BaseDir, year.Value.ToString()),
            (not null, not null) => Path.Combine(BaseDir, year.Value.ToString(), day.Value.ToString("00"))
        };
        fileSystem.CreateDirectoryIfNotExists(path);
        return path;
    }

    private string GetFileName(int? year, int? day, string name) => Path.Combine(GetDirectory(year, day), name);

    public Task<string> ReadFromCache(int? year, int? day, string name)
    {
        logger.LogTrace($"CACHE-READ: {year} - {day} - {name}");
        return fileSystem.ReadAllTextAsync(GetFileName(year, day, name));
    }

    public Task WriteToCache(int? year, int? day, string name, string content)
    {
        logger.LogTrace($"CACHE-WRITE: {year} - {day} - {name}");
        return fileSystem.WriteAllTextAsync(GetFileName(year, day, name), content);
    }

    public bool Exists(int? year, int? day, string name) => fileSystem.FileExists(GetFileName(year, day, name));
}

