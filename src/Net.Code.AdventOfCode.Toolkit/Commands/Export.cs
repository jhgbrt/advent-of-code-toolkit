﻿
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

[Description("Export the code for a puzzle to a stand-alone C# project")]
partial class Export : SinglePuzzleCommand<Export.Settings>
{
    private readonly ICodeManager manager;
    private readonly IInputOutputService output;

    public Export(ICodeManager manager, AoCLogic logic, IInputOutputService output) : base(logic)
    {
        this.manager = manager;
        this.output = output;
    }

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
        var output = options.output;
        var includecommon = options.includecommon;
        string code = await manager.GenerateCodeAsync(key);

        if (string.IsNullOrEmpty(output))
        {
            this.output.WriteLine(code.EscapeMarkup());
        }
        else
        {
            this.output.WriteLine($"Exporting puzzle: {key} to {output}");
            await manager.ExportCode(key, code, includecommon, output);
        }
        return 0;
    }
}



