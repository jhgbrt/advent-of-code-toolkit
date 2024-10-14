using Net.Code.AdventOfCode.Toolkit.Core;

using System.Reflection;

namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

public class AssemblyResolver : IAssemblyResolver
{
    public readonly static IAssemblyResolver Instance = new AssemblyResolver();
    public Assembly? GetEntryAssembly() => Assembly.GetEntryAssembly();
}

class FixedAssemblyResolver(Assembly assembly) : IAssemblyResolver
{
    public Assembly? GetEntryAssembly() => assembly;
}

