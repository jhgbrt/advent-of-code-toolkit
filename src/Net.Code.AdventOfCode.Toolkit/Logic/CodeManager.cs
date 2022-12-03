
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics;
using System.Xml.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class CodeManager : ICodeManager
{
    private readonly IAoCClient client;
    private readonly IFileSystem fileSystem;

    public CodeManager(IAoCClient client, IFileSystem fileSystem)
    {
        this.client = client;
        this.fileSystem = fileSystem;
    }

    public async Task InitializeCodeAsync(int year, int day, bool force, Action<string> progress)
    {
        var codeFolder = fileSystem.GetCodeFolder(year, day);
        var templateDir = fileSystem.GetTemplateFolder();

        if (!templateDir.Exists)
        {
            await templateDir.Initialize();
        }

        if (codeFolder.Exists && !force)
        {
            throw new Exception($"Puzzle for {year}/{day} already initialized. Use --force to re-initialize.");
        }

        await codeFolder.CreateIfNotExists();
        var code = await templateDir.ReadCode(year, day);
        await codeFolder.WriteCode(code);
        await codeFolder.WriteSample("");
        var input = await client.GetPuzzleInputAsync(year, day);
        await codeFolder.WriteInput(input);
        await client.GetPuzzleAsync(year, day, !force);
    }

    public async Task<string> GenerateCodeAsync(int year, int day)
    {
        var dir = fileSystem.GetCodeFolder(year, day);
        var aoc = await dir.ReadCode();
        var tree = CSharpSyntaxTree.ParseText(aoc);

        // find a class with 2 methods without arguments called Part1() and Part2()
        // the members of this class will be converted to top level statements

        (var aocclass, var _) = (
            from classdecl in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            let m =
                from m in classdecl.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where m.Identifier.ToString() is "Part1" or "Part2"
                && m.ParameterList.Parameters.Count == 0
                select m.WithModifiers(TokenList())
            where m.Count() == 2
            select (classdecl, m)
            ).SingleOrDefault();

        if (aocclass is null)
        {
            throw new Exception("Could not find a class with 2 methods called Part1 and Part2");
        }

        // the actual methods: Part1 & Part2
        var implementations = (
            from node in aocclass.DescendantNodes().OfType<MethodDeclarationSyntax>()
            where node.Identifier.ToString() is "Part1" or "Part2"
            from arrow in node.ChildNodes().OfType<ArrowExpressionClauseSyntax>()
            from impl in arrow.ChildNodes().OfType<ExpressionSyntax>()
            select (name: node.Identifier.ToString(), impl)
            ).ToDictionary(x => x.name, x => x.impl);

        // all fields are converted to local declarations
        // the initialization of the input variable is converted to the corresponding System.IO.File call
        var fields =
            from node in aocclass.DescendantNodes().OfType<FieldDeclarationSyntax>()
            let fieldname = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().Identifier.ToString()
            select LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName(
                            Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())
                            )
                        ).WithVariables(
                            SingletonSeparatedList(
                                fieldname != "input"
                                ? node.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single()
                                : VariableDeclarator(
                                    Identifier("input")
                                    ).WithInitializer(
                                        EqualsValueClause(
                                            ConvertInputReaderStatement(node.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Single()
                                        )
                                    )
                                )
                            )
                        )
                    )
                ;

        // methods from the AoC class are converted to top-level methods
        var methods =
            from node in aocclass.DescendantNodes().OfType<MethodDeclarationSyntax>()
            select node.WithModifiers(TokenList())
            ;

        // collect usings, records, enums and other classes
        var usings = tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();

        var records = tree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>();

        var enums = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();

        var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Where(cd => cd != aocclass && !cd.Identifier.ToString().Contains("Tests"));

        // build new compilation unit:
        // - usings
        // - top level variables
        // - top level statements
        // - top level methods
        // - records, classes, enums

        var result = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(
                List(
                    fields.Select(GlobalStatement)
                    .Concat(new[]
                    {
                        ParseMemberDeclaration("var sw = Stopwatch.StartNew();\r\n")!,
                        ParseMemberDeclaration("var part1 = Part1();\r\n")!,
                        ParseMemberDeclaration("var part2 = Part2();\r\n")!,
                    })
                    .Concat(new[]
                    {
                      GlobalStatement(ParseStatement("Console.WriteLine((part1, part2, sw.Elapsed));\r\n")!)
                    })
                    .Concat(List<MemberDeclarationSyntax>(methods))
                    .Concat(List<MemberDeclarationSyntax>(records))
                    .Concat(List<MemberDeclarationSyntax>(classes))
                    .Concat(List<MemberDeclarationSyntax>(enums))
                )
            );
        var workspace = new AdhocWorkspace();
        var code = Formatter.Format(result.NormalizeWhitespace(), workspace, workspace.Options
            .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
            ).ToString();
        return code;
    }
    private static InvocationExpressionSyntax ConvertInputReaderStatement(MemberAccessExpressionSyntax memberAccessExpression)
    {
        if (!memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            throw new NotSupportedException($"Can not convert expression {memberAccessExpression}");

        var methodName = memberAccessExpression.ToString() switch
        {
            "Read.InputLines" => "ReadAllLines",
            "Read.InputText" => "ReadAllText",
            _ => throw new NotSupportedException($"Can not convert expression {memberAccessExpression}")
        };

        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("File"),
                IdentifierName(methodName)
                )
            )
        .WithArgumentList(
            ArgumentList(
            SingletonSeparatedList(
                Argument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal("input.txt")
                        )
                    )
                )
            )
        );
    }

    public async Task ExportCode(int year, int day, string code, string output)
    {
        var codeDir = fileSystem.GetCodeFolder(year, day);
        var outputDir = fileSystem.GetOutputFolder(output);
        var templateDir = fileSystem.GetTemplateFolder();
        await outputDir.CreateIfNotExists();
        await outputDir.WriteCode(code);
        outputDir.CopyFiles(codeDir.GetCodeFiles());
        outputDir.CopyFile(codeDir.Input);
        outputDir.CopyFile(templateDir.CsProj);
    }
}