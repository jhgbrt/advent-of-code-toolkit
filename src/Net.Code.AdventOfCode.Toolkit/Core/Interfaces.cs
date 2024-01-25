namespace Net.Code.AdventOfCode.Toolkit.Core;
using System.Net;
using System.Reflection;
using Net.Code.AdventOfCode.Toolkit.Core.Leaderboard;
using Net.Code.AdventOfCode.Toolkit.Logic;

record Configuration(string BaseAddress, string SessionCookie);

interface IAoCClient : IDisposable
{
    Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id);
    Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(int year);
    Task<PersonalStats?> GetPersonalStatsAsync(int year);
    Task<int> GetMemberId();
    Task<Puzzle> GetPuzzleAsync(PuzzleKey key);
    Task<string> GetPuzzleInputAsync(PuzzleKey key);
    Task<(HttpStatusCode status, string content)> PostAnswerAsync(int year, int day, int part, string value);
}

interface IAoCRunner
{
    Task<DayResult?> Run(string? typeName, PuzzleKey key, Action<int, Result> progress);
}

interface ICodeManager
{
    Task ExportCode(PuzzleKey key, string code, string[]? includecommon, string output);
    Task<string> GenerateCodeAsync(PuzzleKey key);
    Task InitializeCodeAsync(Puzzle puzzle, bool force, string? template, Action<string> progress);
    Task SyncPuzzleAsync(Puzzle puzzle);
}

interface IPuzzleManager
{
    Task<Puzzle> SyncPuzzle(PuzzleKey key);
    Task<Puzzle> GetPuzzle(PuzzleKey key);
    Task<PuzzleResultStatus[]> GetPuzzleResults(int? year, TimeSpan? slowerthan);
    Task<PuzzleResultStatus> GetPuzzleResult(PuzzleKey key);
    Task AddResult(DayResult result);
    Task<(bool success, string content)> PostAnswer(PuzzleKey key, string answer);
}

interface ILeaderboardManager
{
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardsAsync(int id, IEnumerable<int> years);
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int id, int year);
    Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(int year);
    IAsyncEnumerable<MemberStats> GetMemberStats(IEnumerable<int> years);
}

interface IFileSystemFactory
{
    ICodeFolder GetCodeFolder(PuzzleKey key);
    IFolder GetFolder(string name);
    ITemplateFolder GetTemplateFolder(string? template);
    IOutputFolder GetOutputFolder(string output);

}

interface IFileSystem
{
    string CurrentDirectory { get; }
    void CreateDirectoryIfNotExists(string path, FileAttributes? attributes = default);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void DeleteFile(string path);
}
interface IOutputFolder
{
    void CopyFile(FileInfo source, string? subfolder = null);
    void CopyFiles(IEnumerable<FileInfo> sources, string? subfolder = null);
    Task CreateIfNotExists(string? subfolder = null);
    Task WriteCode(string code);
}
interface IFolder
{
    bool Exists { get; }
    Task<string> ReadFile(string name);
    IEnumerable<FileInfo> GetFiles(string pattern);
}
interface ICodeFolder
{
    void CopyFile(FileInfo source, string? subfolder = null);
    FileInfo Input { get; }
    FileInfo Sample { get; }
    Task CreateIfNotExists(string? subfolder = null);
    IEnumerable<FileInfo> GetCodeFiles();
    Task<string> ReadCode();
    Task WriteCode(string content);
    Task WriteInput(string content);
    Task WriteSample(string content);
    bool Exists { get; }
}
interface ITemplateFolder
{
    FileInfo Code { get; }
    FileInfo CsProj { get; }
    FileInfo Notebook { get; }
    FileInfo Sample { get; }
    bool Exists { get; }
    Task<string> ReadCode(PuzzleKey key);
}
interface IHttpClientWrapper : IDisposable
{
    Task<(HttpStatusCode status, string content)> PostAsync(string path, HttpContent body);
    Task<(HttpStatusCode status, string content)> GetAsync(string path);
}

public interface IAssemblyResolver
{
    Assembly? GetEntryAssembly();
}
internal interface IAoCDbContext: IDisposable
{
    void AddPuzzle(Puzzle puzzle);
    void AddResult(DayResult result);
    ValueTask<Puzzle?> GetPuzzle(PuzzleKey key);
    ValueTask<DayResult?> GetResult(PuzzleKey key);
    void Migrate();
    Task<int> SaveChangesAsync(CancellationToken token = default);

    IQueryable<Puzzle> Puzzles { get; }
    IQueryable<DayResult> Results { get; }
}