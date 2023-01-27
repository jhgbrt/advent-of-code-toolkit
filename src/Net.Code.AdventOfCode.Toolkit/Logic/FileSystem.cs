
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Logic;



static class Directories
{
    private readonly static FileName CODE = new("aoc.cs");
    private readonly static FileName CSPROJ = new("aoc.csproj");
    private readonly static FileName NOTEBOOK = new("aoc.ipynb");
    private readonly static FileName INPUT = new("input.txt");
    private readonly static FileName SAMPLE = new("sample.txt");
    public static FileInfo GetCodeFile(this DirectoryInfo dir) => dir.GetFile(CODE);
    public static FileInfo GetCsprojFile(this DirectoryInfo dir) => dir.GetFile(CSPROJ);
    public static FileInfo GetNotebookFile(this DirectoryInfo dir) => dir.GetFile(NOTEBOOK);
    public static FileInfo GetInputFile(this DirectoryInfo dir) => dir.GetFile(INPUT);
    public static FileInfo GetSampleFile(this DirectoryInfo dir) => dir.GetFile(SAMPLE);


    public static DirectoryInfo GetCodeFolder(this IFileSystem filesystem, PuzzleKey key)
        => filesystem.CurrentDirectory.GetDirectory($"Year{key.Year}").GetDirectory($"Day{key.Day:00}");
    public static DirectoryInfo GetTemplateFolder(this IFileSystem filesystem) 
        => filesystem.CurrentDirectory.GetDirectory("Template");
}

class FileSystem : IFileSystem
{
    public DirectoryInfo CurrentDirectory => new(Environment.CurrentDirectory);
    private readonly ILogger<FileSystem> logger;
    public FileSystem(ILogger<FileSystem> logger)
    {
        this.logger = logger;
    }
    public void Create(DirectoryInfo path)
    {
        var dir = new System.IO.DirectoryInfo(path.FullName);
        if (!dir.Exists)
        {
            logger.LogTrace($"CREATE: {path}");
            dir.Create();
        }
    }
    public async Task<string> Read(FileInfo path)
    {
        logger.LogTrace($"READ: {path}");
        return await System.IO.File.ReadAllTextAsync(path.FullName);
    }

    public async Task Write(FileInfo path, string content)
    {
        if (Exists(path))
            Delete(path);
        logger.LogTrace($"WRITE: {path}");
        await System.IO.File.WriteAllTextAsync(path.FullName, content);
    }

    public bool Exists(FileInfo path)
    {
        return System.IO.File.Exists(path.FullName);
    }

    public bool Exists(DirectoryInfo path)
    {
        return System.IO.Directory.Exists(path.FullName);
    }

    public void Delete(DirectoryInfo path)
    {
        logger.LogTrace($"DELETE: {path}");
        System.IO.Directory.Delete(path.FullName);
    }

    public void Delete(FileInfo path)
    {
        logger.LogTrace($"DELETE: {path}");
        System.IO.File.Delete(path.FullName);
    }

    public IEnumerable<FileInfo> GetFiles(DirectoryInfo dir, string search)
        => from fullname in System.IO.Directory.GetFiles(dir.FullName, search)
           let filename = new FileName(System.IO.Path.GetFileName(fullname))
           let directory = new DirectoryInfo(System.IO.Path.GetDirectoryName(fullname)!)
           select new FileInfo(directory, filename);
    
    public void Copy(IEnumerable<FileInfo> sources, DirectoryInfo destination)
    {
        foreach (var source in sources) Copy(source, destination);
    }

    public void Copy(FileInfo source, DirectoryInfo destination)
    {
        Copy(source, new FileInfo(destination, source.Name));
    }
    public void Copy(FileInfo source, FileInfo destination)
    {
        logger.LogTrace($"COPY: {source} -> {destination}");
        new System.IO.FileInfo(source.FullName).CopyTo(destination.FullName, true);
    }
}