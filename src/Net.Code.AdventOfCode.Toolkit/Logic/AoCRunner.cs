
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

public class AssemblyResolver : IAssemblyResolver
{
    public static IAssemblyResolver Instance = new AssemblyResolver();
    public Assembly? GetEntryAssembly() => Assembly.GetEntryAssembly();
}
class AoCRunner : IAoCRunner
{
    ILogger<AoCRunner> logger;
    private readonly ICache cache;
    private readonly IAssemblyResolver resolver;

    public AoCRunner(ILogger<AoCRunner> logger, ICache cache, IAssemblyResolver resolver)
    {
        this.logger = logger;
        this.cache = cache;
        this.resolver = resolver;
    }

    public async Task<DayResult> Run(string? typeName, int year, int day, Action<int, Result> progress)
    {
        dynamic? aoc = GetAoC(typeName, year, day);

        if (aoc == null)
        {
            return DayResult.NotImplemented(year, day);
        }

        var t1 = Run(() => aoc.Part1());
        logger.LogDebug($"{year}/{day}, Part 1: result = {t1.Value} - {t1.Elapsed}");
        progress(1, t1);

        var t2 = day < 25
            ? Run(() => aoc.Part2())
            : new Result(ResultStatus.Ok, "", TimeSpan.Zero);
        logger.LogDebug($"{year}/{day}, Part 2: result = {t1.Value} - {t1.Elapsed}");
        progress(2, t2);

        var result = new DayResult(year, day, t1, t2);

        await cache.WriteToCache(year, day, "result.json", JsonSerializer.Serialize(result));

        return result;
    }

    private dynamic? GetAoC(string? typeName, int year, int day)
    {
        var assembly = resolver.GetEntryAssembly();
        if (assembly == null) throw new Exception("no entry assembly?");

        Type? type = null;
        if (string.IsNullOrEmpty(typeName))
        {
            foreach (var t in assembly.GetTypes().OrderBy(t => t.Name))
            {
                if (t.Name.Contains('<')) // skip compiler-generated classes
                    continue;

                var name = t.FullName ?? t.Name;
                if (t.Namespace is null || !t.Namespace.Contains($"{year}") || !t.Namespace.Replace($"{year}", "").ToLower().Contains($"day{day:00}"))
                {
                    continue;
                }

                logger.LogDebug($"considered {t.FullName}");
                var methods = t.GetMethods();
                if (!methods.Any(m => m.Name == "Part1" && m.GetParameters().Length == 0))
                {
                    logger.LogDebug($"{t.Name} does not have a parameterless method Part1()");
                    continue;
                }
                if (!methods.Any(m => m.Name == "Part2" && m.GetParameters().Length == 0))
                {
                    logger.LogDebug($"{t.Name} does not have a parameterless method Part2()");
                    continue;
                }
                type = t;
                break;
            }
        }
        else
        {
            type = assembly.GetType(string.Format(typeName, year, day));
        }

        if (type is null)
        {
            logger.LogWarning($"No AoC implementation found for {year}/{day}");
            return null;
        }

        logger.LogInformation($"Using implementation type: {type}");

        return Activator.CreateInstance(type);
    }

    static Result Run(Func<object> f)
    {
        var sw = Stopwatch.StartNew();
        var result = f();
        return result is "" ? Result.Empty : new Result(ResultStatus.Unknown, result.ToString() ?? string.Empty, sw.Elapsed);
    }

}


