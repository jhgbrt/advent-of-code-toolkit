using System.Runtime.CompilerServices;

namespace AdventOfCode;

internal static class Read
{
    public static string SampleText([CallerFilePath] string type = "") => Text(type, "sample.txt");
    public static string[] SampleLines([CallerFilePath] string type = "") => Lines(type, "sample.txt").ToArray();
    public static string InputText([CallerFilePath] string type = "") => Text(type, "input.txt");
    public static string[] InputLines([CallerFilePath] string type = "") => Lines(type, "input.txt").ToArray();
    public static StreamReader InputStream([CallerFilePath] string type = "") => new StreamReader(Stream(type, "input.txt"));
    public static string Text(string type, string filename) => File.ReadAllText(Path.Combine(Path.GetDirectoryName(type) ?? "", filename));
    public static IEnumerable<string> Lines(string type, string filename) => File.ReadLines(Path.Combine(Path.GetDirectoryName(type) ?? "", filename));
    public static Stream Stream(string type, string filename) => File.OpenRead(Path.Combine(Path.GetDirectoryName(type) ?? "", filename));
}
