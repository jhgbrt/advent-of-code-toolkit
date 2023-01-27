namespace Net.Code.AdventOfCode.Toolkit.Core;
using System.Net;
using System.Reflection;

using Net.Code.AdventOfCode.Toolkit.Core.Leaderboard;

record Configuration(string BaseAddress, string SessionCookie);

interface IAoCClient : IDisposable
{
    Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id);
    Task<IEnumerable<(int id, string description)>> GetLeaderboardIds();
    Task<PersonalStats?> GetPersonalStatsAsync(int year);
    Task<int> GetMemberId();
    Task<Puzzle> GetPuzzleAsync(PuzzleKey key);
    Task<string> GetPuzzleInputAsync(PuzzleKey key);
    Task<(HttpStatusCode status, string content)> PostAnswerAsync(int year, int day, int part, string value);
}

interface IAoCRunner
{
    Task Test(string? typeName, PuzzleKey key, Action<int, Result> progress);
    Task<DayResult?> Run(string? typeName, PuzzleKey key, Action<int, Result> progress);
}

interface ICodeManager
{
    Task ExportCode(PuzzleKey key, string code, bool includecommon, string output);
    Task<string> GenerateCodeAsync(PuzzleKey key);
    Task InitializeCodeAsync(Puzzle puzzle, bool force, Action<string> progress);
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
    Task<IEnumerable<(int id, string description)>> GetLeaderboardIds();
    IAsyncEnumerable<MemberStats> GetMemberStats(IEnumerable<int> years);
}
readonly record struct FileName(string Name)
{
    public override string ToString() => Name;
}
readonly record struct FileInfo(DirectoryInfo Directory, FileName Name)
{
    public string FullName => System.IO.Path.Combine(Directory.FullName, Name.Name);
    public override string ToString() => FullName;
}

readonly record struct DirectoryInfo(string FullName)
{
    public override string ToString() => FullName;
    public DirectoryInfo GetDirectory(string name) => new(System.IO.Path.Combine(FullName, name));

    internal FileInfo GetFile(FileName name) => new FileInfo(this, name);
}
interface IFileSystem
{
    DirectoryInfo CurrentDirectory { get; }
    void Create(DirectoryInfo dir);
    Task<string> Read(FileInfo file);
    Task Write(FileInfo file, string content);
    bool Exists(FileInfo file);
    bool Exists(DirectoryInfo dir);
    void Delete(DirectoryInfo dir);
    void Delete(FileInfo filie);
    IEnumerable<FileInfo> GetFiles(DirectoryInfo dir, string pattern);
    void Copy(IEnumerable<FileInfo> sources, DirectoryInfo destination);
    void Copy(FileInfo source, DirectoryInfo destination);
    void Copy(FileInfo source, FileInfo destination);
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
internal interface IAoCDbContext : IDisposable
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