
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Export the code for a puzzle to a stand-alone C# project")]
class Export(ICodeManager manager, AoCLogic logic, IInputOutputService output) : SinglePuzzleCommand<Export.Settings>(logic)
{
    public class Settings : AoCSettings
    {
        [Description("output location. If empty, exported code is written to stdout")]
        [CommandOption("-o|--output")]
        public string? output { get; set; }
        [Description("Include common code. If true, all code files included in 'common' are also exported.")]
        [CommandOption("-c|--include-common")]
        public string[]? includecommon { get; set; }
    }
    public override async Task<int> ExecuteAsync(PuzzleKey key, Settings options)
    {
        var includecommon = options.includecommon;
        string code = await manager.GenerateCodeAsync(key);

        if (string.IsNullOrEmpty(options.output))
        {
            output.WriteLine(code.EscapeMarkup());
        }
        else
        {
            output.WriteLine($"Exporting puzzle: {key} to {options.output}");
            await manager.ExportCode(key, code, includecommon, options.output);
        }
        return 0;
    }
}



