
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class AoCRunner : IAoCRunner
{
    ILogger<AoCRunner> logger;
    private readonly IAssemblyResolver resolver;

    public AoCRunner(ILogger<AoCRunner> logger, IAssemblyResolver resolver)
    {
        this.logger = logger;
        this.resolver = resolver;
    }

    public async Task<DayResult?> Run(string? typeName, PuzzleKey key, Action<int, Result> progress)
    {
        var (year, day) = key;
        dynamic? aoc = GetAoC(typeName, year, day);

        if (aoc == null)
        {
            return null;
        }

        var t1 = await Run(() => aoc.Part1());
        logger.LogDebug($"{key}, Part 1: result = {t1.Value} - {t1.Elapsed}");
        progress(1, t1);

        var t2 = day < 25
            ? await Run(() => aoc.Part2())
            : new Result(ResultStatus.Ok, "", TimeSpan.Zero);
        logger.LogDebug($"{key}, Part 2: result = {t2.Value} - {t2.Elapsed}");
        progress(2, t2);

        var result = new DayResult(key, t1, t2);

        return result;
    }
    bool IsCompilerGenerated(Type type) 
    => (type?.FullName??string.Empty).Contains('<') 
    || type?.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
    private dynamic? GetAoC(string? typeName, int year, int day)
    {
        var assembly = resolver.GetEntryAssembly();
        if (assembly == null) throw new Exception("no entry assembly?");

        string yearday = $"{year}{day:00}";

        Type? type = null;
        MethodInfo? part1 = null;
        MethodInfo? part2 = null;
        if (string.IsNullOrEmpty(typeName))
        {
            var types = from t in assembly.GetTypes()
                        where (!IsCompilerGenerated(t))
                        && (t?.FullName ?? string.Empty).EndsWith(yearday)
                        select t;

            foreach (var t in types)
            {

                logger.LogDebug("considered {typeName}", t.FullName);

                var methods = t.GetMethods() ?? [];
                var method1 = methods.FirstOrDefault(m => m.Name is "Part1" && m.GetParameters().Length is 0);
                var method2 = methods.FirstOrDefault(m => m.Name is "Part2" && m.GetParameters().Length is 0);

                if (method1 is null || method2 is null)
                {
                    logger.LogDebug($"{t?.Name} does not have two parameterless methods Part1() and Part2()");
                    continue;
                }

                (type, part1, part2) = (t, method1, method2);
                break;
            }
        }
        else
        {
            type = assembly.GetType(string.Format(typeName, year, day));
            var methods = type?.GetMethods() ?? [];
            part1 = methods.FirstOrDefault(m => m.Name == "Part1" && m.GetParameters().Length == 0);
            part2 = methods.FirstOrDefault(m => m.Name == "Part2" && m.GetParameters().Length == 0);
        }

        if (type is null || part1 is null || part2 is null)
        {
            logger.LogWarning("No AoC implementation found for {year}/{day}", year, day);
            return null;
        }

        logger.LogInformation("Using implementation type: {type}", type.FullName);

        if (type.IsAbstract)
            return new Runner(part1, part2);
        else
            return Activator.CreateInstance(type);
    }
    class Runner
    {
        readonly Func<object> part1;
        readonly Func<object> part2;
        public Runner(MethodInfo part1, MethodInfo part2)
        {
            this.part1 = part1.CreateDelegate<Func<object>>();
            this.part2 = part2.CreateDelegate<Func<object>>();
        }
        public object Part1() => part1();
        public object Part2() => part2();
    }
   
    static async Task<Result> Run(Func<object> f)
    {
        var sw = Stopwatch.StartNew();
        var result = await Task.Run(f);
        return result is null or "" or -1 ? Result.Empty : new Result(ResultStatus.Unknown, result.ToString() ?? string.Empty, sw.Elapsed);
    }

}


