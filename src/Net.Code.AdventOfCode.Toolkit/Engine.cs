namespace Net.Code.AdventOfCode.Toolkit;
using Net.Code.AdventOfCode.Toolkit.Core;

public interface IEngine
{
    Task<(bool, string)> GetResultAsync(int year, int day);
}

class Engine : IEngine
{
    private readonly IPuzzleManager manager;

    public Engine(IPuzzleManager manager)
    {
        this.manager = manager;
    }

    /// <summary>
    /// This method retrieves the last result from the cache for a specific puzzle. It won't fetch online data.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="day"></param>
    /// <returns>(bool ok, string message): ok is true when the puzzle result is verifiably correct; false otherwise. The message gives more info.</returns>
    public async Task<(bool, string)> GetResultAsync(int year, int day)
    {
        var status = await manager.GetPuzzleResult(new(year, day));
        return (status.Ok, status.ToReportLine());
    }
}
