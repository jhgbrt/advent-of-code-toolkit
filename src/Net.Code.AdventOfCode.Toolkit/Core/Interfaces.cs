namespace Net.Code.AdventOfCode.Toolkit.Core;
using System.Net;
using System.Reflection;
record Configuration(string BaseAddress, string SessionCookie);

interface IAoCClient : IDisposable
{
    Task<LeaderBoard?> GetLeaderBoardAsync(int year, int id);
    Task<IEnumerable<(int id, string description)>> GetLeaderboardIds();
    Task<Member?> GetMemberAsync(int year);
    Task<int> GetMemberId();
    Task<Puzzle> GetPuzzleAsync(PuzzleKey key);
    Task<string> GetPuzzleInputAsync(PuzzleKey key);
    Task<(HttpStatusCode status, string content)> PostAnswerAsync(int year, int day, int part, string value);
}

interface IAoCRunner
{
    Task<DayResult?> Run(string? typeName, PuzzleKey key, Action<int, Result> progress);
}
interface ICache
{
    bool Exists(int? year, int? day, string name);
    Task<string> ReadFromCache(int? year, int? day, string name);
    Task WriteToCache(int? year, int? day, string name, string content);
}

interface ICodeManager
{
    Task ExportCode(int year, int day, string code, bool includecommon, string output);
    Task<string> GenerateCodeAsync(int year, int day);
    Task InitializeCodeAsync(Puzzle puzzle, bool force, Action<string> progress);
    Task SyncPuzzleAsync(Puzzle puzzle);
}

interface IPuzzleManager
{
    Task<Puzzle> GetPuzzle(PuzzleKey key);
    Task<PuzzleResultStatus[]> GetPuzzleResults(int? year, TimeSpan? slowerthan);
    Task<PuzzleResultStatus> GetPuzzleResult(PuzzleKey key);
    Task SaveResult(DayResult result);
    Task<(bool success, string content)> PostAnswer(PuzzleKey key, AnswerToPost answer);
    Task<(bool status, string reason, int part)> PreparePost(PuzzleKey key);
}

interface ILeaderboardManager
{
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardsAsync(int id, IEnumerable<int> years);
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int id);
    Task<IEnumerable<(int id, string description)>> GetLeaderboardIds(bool usecache);
}

interface IMemberManager
{
    IAsyncEnumerable<(int year, MemberStats stats)> GetMemberStats(IEnumerable<int> years);
}

interface IReportManager
{
    Task<IEnumerable<PuzzleReportEntry>> GetPuzzleReport(ResultStatus? status, int? slowerthan, int? year);
}
interface IFileSystem
{
    string CurrentDirectory { get; }
    ICodeFolder GetCodeFolder(int year, int day);
    IFolder GetFolder(string name);
    ITemplateFolder GetTemplateFolder();
    IOutputFolder GetOutputFolder(string output);
    void CreateDirectoryIfNotExists(string path, FileAttributes? attributes = default);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
    bool FileExists(string path);
    bool DirectoryExists(string path);
}
interface IOutputFolder
{
    void CopyFile(FileInfo source);
    void CopyFiles(IEnumerable<FileInfo> sources);
    Task CreateIfNotExists();
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
    void CopyFile(FileInfo source);
    FileInfo Input { get; }
    FileInfo Sample { get; }
    Task CreateIfNotExists();
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
    Task<string> ReadCode(int year, int day);
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
