using Net.Code.AdventOfCode.Toolkit.Core;

using System.Reflection;

namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

public class AssemblyResolver : IAssemblyResolver
{
    public static IAssemblyResolver Instance = new AssemblyResolver();
    public Assembly? GetEntryAssembly() => Assembly.GetEntryAssembly();
}

class FixedAssemblyResolver : IAssemblyResolver
{
    private Assembly assembly;

    public FixedAssemblyResolver(Assembly assembly)
    {
        this.assembly = assembly;
    }

    public Assembly? GetEntryAssembly() => assembly;
}

