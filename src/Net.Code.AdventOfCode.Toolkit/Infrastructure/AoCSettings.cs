using Microsoft.Extensions.Logging;

using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

public interface IAoCSettings
{
#pragma warning disable IDE1006 // Naming Styles
    public int? year { get; }
    public int? day { get; }
#pragma warning restore IDE1006 // Naming Styles
}

public interface IManyPuzzleSettings : IAoCSettings
{
    bool all { get; }
}

public class AoCSettings : CommandSettings, IManyPuzzleSettings, IAoCSettings
{
    [Description("Year (default: current year)")]
    [CommandArgument(0, "[YEAR]")]
    public int? year { get; set; }
    [Description("Day (default during advent: current day, null otherwise)")]
    [CommandArgument(1, "[DAY]")]
    public int? day { get; set; }
    [Description("Force executing this command for all puzzles")]
    [CommandOption("--all")]
    public bool all { get; set; }
    [Description("Set the log level. Valid values: Trace, Debug, Information, Warning, Error")]
    [CommandOption("--loglevel")]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    [Description("Run in debug mode.")]
    [CommandOption("--debug")]
    public bool Debug { get; set; }

}
