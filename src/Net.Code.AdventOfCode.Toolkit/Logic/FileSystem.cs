
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics.Tracing;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class FileSystem : IFileSystem
{
    public string CurrentDirectory => Environment.CurrentDirectory;
    private readonly ILogger<FileSystem> logger;
    public FileSystem(ILogger<FileSystem> logger)
    {
        this.logger = logger;
    }

    public ICodeFolder GetCodeFolder(int year, int day) => new CodeFolder(Path.Combine(CurrentDirectory, $"Year{year}", $"Day{day:00}"), logger);
    public IFolder GetFolder(string name) => new Folder(Path.Combine(CurrentDirectory, name), logger);
    public ITemplateFolder GetTemplateFolder() => new TemplateFolder(Path.Combine(CurrentDirectory, "Template"), logger);
    public IOutputFolder GetOutputFolder(string output) => new OutputFolder(output, logger);

    public void CreateDirectoryIfNotExists(string path, FileAttributes? attributes)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists) dir.Create();
        if (attributes.HasValue)
            dir.Attributes |= attributes.Value;
    }
    public async Task<string> ReadAllTextAsync(string path) => await File.ReadAllTextAsync(path);
    public async Task WriteAllTextAsync(string path, string content) => await File.WriteAllTextAsync(path, content);
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    class Folder : IFolder
    {
        private readonly DirectoryInfo dir;
        protected readonly ILogger logger;
        public Folder(string path, ILogger logger) { dir = new DirectoryInfo(path); this.logger = logger; }
        public DirectoryInfo Directory => dir;
        public string GetFileName(string file) => Path.Combine(dir.FullName, file);
        public bool Exists => dir.Exists;
        public Task CreateIfNotExists()
        {
            if (!dir.Exists)
            {
                logger.LogTrace($"CREATE: {dir}");
                dir.Create();
            }
            return Task.CompletedTask;
        }
        protected Task Delete()
        {
            logger.LogTrace($"DELETE: {dir}");
            dir.Delete(true);
            return Task.CompletedTask;
        }
        public Task<string> ReadFile(string name)
        {
            logger.LogTrace($"READ: {name}");
            return File.ReadAllTextAsync(name);
        }
        protected Task WriteFile(string name, string content)
        {
            DeleteIfExists(name);
            logger.LogTrace($"WRITE: {name}");
            return File.WriteAllTextAsync(name, content);
        }
        protected Task DeleteIfExists(string name)
        {
            var n = GetFileName(name);
            if (File.Exists(n))
            {
                logger.LogTrace($"DELETE: {n}");
                File.Delete(n);
            }
            return Task.CompletedTask;
        }
        public void CopyFile(FileInfo source)
        {
            var n = GetFileName(source.Name);
            logger.LogTrace($"COPY: {source} -> {n}");
            source.CopyTo(n, true);
        }

        public IEnumerable<FileInfo> GetFiles(string pattern) => dir.GetFiles(pattern);
        public override string ToString() => dir.FullName;
    }
    class OutputFolder : Folder, IOutputFolder
    {
        public OutputFolder(string location, ILogger logger) : base(location, logger) { }
        private string CODE => GetFileName("aoc.cs");
        private string INPUT => GetFileName("input.txt");
        private string CSPROJ => GetFileName("aoc.csproj");
        public FileInfo Code => new FileInfo(CODE);
        public FileInfo Input => new FileInfo(INPUT);
        public FileInfo CsProj => new FileInfo(CSPROJ);
        public async Task WriteCode(string code) => await WriteFile(CODE, code);

        public new Task CreateIfNotExists() => base.CreateIfNotExists();

        public void CopyFiles(IEnumerable<FileInfo> sources)
        {
            foreach (var source in sources) CopyFile(source);
        }
    }
    class CodeFolder : Folder, ICodeFolder
    {
        public CodeFolder(string path, ILogger logger)
            : base(path, logger)
        {
        }

        public string CODE => GetFileName("AoC.cs");
        public string INPUT => GetFileName("input.txt");
        public string SAMPLE => GetFileName("sample.txt");
        public FileInfo Input => new FileInfo(INPUT);
        public FileInfo Sample => new FileInfo(SAMPLE);
        public Task<string> ReadCode() => ReadFile(CODE);
        public Task WriteCode(string content) => WriteFile(CODE, content);
        public Task WriteInput(string content) => WriteFile(INPUT, content);
        public Task WriteSample(string content) => WriteFile(SAMPLE, content);

        public IEnumerable<FileInfo> GetCodeFiles() => GetFiles("*.cs").Where(f => !f.FullName.Equals(CODE, StringComparison.OrdinalIgnoreCase));
    }
    class TemplateFolder : Folder, ITemplateFolder
    {
        public TemplateFolder(string path, ILogger logger)
            : base(path, logger)
        {
        }
        private string CODE => GetFileName("AoC.cs");
        private string NOTEBOOK => GetFileName("aoc.ipynb");
        private string CSPROJ => GetFileName("aoc.csproj");
        public FileInfo Code => new FileInfo(CODE);
        public FileInfo CsProj => new FileInfo(CSPROJ);
        public FileInfo Notebook => new FileInfo(NOTEBOOK);
        public async Task<string> ReadCode(int year, int day)
        {
            var template = await ReadFile(CODE);
            return template.Replace("YYYY", year.ToString()).Replace("DD", day.ToString("00"));
        }
    }
}
