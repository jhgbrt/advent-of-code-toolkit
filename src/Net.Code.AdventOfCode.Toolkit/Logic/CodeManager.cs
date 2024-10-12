using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class CodeManager(IFileSystemFactory fileSystem) : ICodeManager
{
    public async Task InitializeCodeAsync(Puzzle puzzle, bool force, string? template, Action<string> progress)
    {
        var codeFolder = fileSystem.GetCodeFolder(puzzle.Key);
        var templateDir = fileSystem.GetTemplateFolder(template);

        if (!templateDir.Exists)
        {
            throw new AoCException($"Template folder for {template??"default"} template not found.");
        }

        if (codeFolder.Exists && !force)
        {
            throw new AoCException($"Puzzle for {puzzle.Key} already initialized. Use --force to re-initialize.");
        }

        var input = puzzle.Input;

        await codeFolder.CreateIfNotExists();

        var code = await templateDir.ReadCode(puzzle.Key);
        await codeFolder.WriteCode(code);
        if (templateDir.Sample.Exists)
        {
            codeFolder.CopyFile(templateDir.Sample);
        }
        else
        {
            await codeFolder.WriteSample("");
        }
        await codeFolder.WriteInput(input);
        if (templateDir.Notebook.Exists)
        {
            codeFolder.CopyFile(templateDir.Notebook);
        }
    }

    public async Task SyncPuzzleAsync(Puzzle puzzle)
    {
        var codeFolder = fileSystem.GetCodeFolder(puzzle.Key);
        await codeFolder.WriteInput(puzzle.Input);
    }

    public async Task<string> GenerateCodeAsync(PuzzleKey key)
    {
        var dir = fileSystem.GetCodeFolder(key);
        var aoc = await dir.ReadCode();
        var tree = CSharpSyntaxTree.ParseText(aoc);

        tree = tree.WithRootAndOptions(tree.GetRoot(), tree.Options);

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


        aocclass = AdjustInputReading(aocclass);

        var constructors = (from c in aocclass.DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                            select c).ToArray();

        IEnumerable<StatementSyntax> initialization = Array.Empty<StatementSyntax>();
        if (constructors.Length > 0)
        {

            var initializerArguments = (
                from c in constructors
                where c.ParameterList.Parameters.Count == 0
                let initializer = c.DescendantNodes().OfType<ConstructorInitializerSyntax>().FirstOrDefault()
                where initializer is not null
                let a = initializer.ArgumentList.Arguments
                select a).FirstOrDefault();

            var constructor = (
                from c in constructors
                where c.ParameterList.Parameters.Count == initializerArguments.Count
                select c
                ).Single();

            initialization = (
                from item in initializerArguments.Zip(constructor.ParameterList.Parameters)
                let value = item.First
                let name = item.Second.Identifier.Value
                select ParseStatement($"var {name} = {value};")
             ).Concat(
                from statement in constructor.DescendantNodes().OfType<BlockSyntax>().First().ChildNodes().OfType<StatementSyntax>()
                where !IsSimpleThisAssignment(statement)
                select ConvertConstructorInitializationStatement(statement)
             ).ToArray();
        }

        // the actual methods: Part1 & Part2
        var implementations = (
            from node in aocclass.DescendantNodes().OfType<MethodDeclarationSyntax>()
            where node.Parent == aocclass
            where node.Identifier.ToString() is "Part1" or "Part2"
            && node.ParameterList.Parameters.Count == 0
            from arrow in node.ChildNodes().OfType<ArrowExpressionClauseSyntax>()
            from impl in arrow.ChildNodes().OfType<ExpressionSyntax>()
            select (name: node.Identifier.ToString(), impl)
            ).ToDictionary(x => x.name, x => x.impl);

        // all fields are converted to local declarations
        // the initialization of the input variable is converted to the corresponding System.IO.File call
        var fields =
            from node in aocclass.DescendantNodes().OfType<FieldDeclarationSyntax>()
            where node.Parent == aocclass
            && !IsInitialized(node, initialization)
            select ToLocalDeclaration(node);


        Debugger.Break();

        // methods from the AoC class are converted to top-level methods
        var methods =
            from node in aocclass.DescendantNodes().OfType<MethodDeclarationSyntax>()
            where node.Parent == aocclass
            where !node.AttributeLists.Any(al => !al.Attributes.Any(a => a.Name.ToString() is "Fact" or "Theory"))
            && !(implementations.ContainsKey(node.Identifier.Text) && node.ParameterList.Parameters.Count == 0)
            select node.WithModifiers(TokenList())
            ;

        // collect usings, records, enums and other classes
        var usings = tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();

        var records = tree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>();

        var enums = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();

        var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Where(cd => cd.Identifier.Value != aocclass.Identifier.Value && !cd.Identifier.ToString().Contains("Tests"));


            // build new compilation unit:
            // - usings
            // - top level variables
            // - top level statements
            // - top level methods
            // - records, classes, enums

            var result = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(
                List(Enumerable.Empty<GlobalStatementSyntax>()
                    .Concat(fields.Select(GlobalStatement))
                    .Concat(initialization.Select(GlobalStatement))
                    .Concat(new[]
                    {
                        GlobalStatement(ParseStatement("var sw = Stopwatch.StartNew();\r\n")!),
                        GenerateGlobalStatement(1, implementations),
                        GenerateGlobalStatement(2, implementations)
                    })
                    .Concat(new[]
                    {
                        GlobalStatement(ParseStatement("Console.WriteLine((part1, part2, sw.Elapsed));\r\n")!),
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

    private bool IsInitialized(FieldDeclarationSyntax node, IEnumerable<StatementSyntax> initialization)
    {
        return initialization.OfType<LocalDeclarationStatementSyntax>().Any(ld => IsInitializationFor(ld, node));
    }

    private bool IsInitializationFor(LocalDeclarationStatementSyntax ld, FieldDeclarationSyntax node)
    {
        if (ld.ChildNodes().First() is not VariableDeclarationSyntax child) return false;
        if (node.Declaration.Variables.Count != 1) return false;
        return ld.DescendantNodes().OfType<VariableDeclaratorSyntax>().Any(n => n.Identifier.Text == node.Declaration.Variables.Single().Identifier.Text);
    }

    private bool IsSimpleThisAssignment(StatementSyntax statement)
    {
        if (statement is not ExpressionStatementSyntax ess) return false;
        var child = ess.ChildNodes().Single();
        if (child is not AssignmentExpressionSyntax assignment) return false;
        if (assignment.Left is not MemberAccessExpressionSyntax left) return false;
        if (assignment.Right is not IdentifierNameSyntax right) return false;
        return left.Name.ToString().Equals(right.ToString());
    }

    // Converts 'a = b' to 'var a = b'
    private StatementSyntax ConvertConstructorInitializationStatement(StatementSyntax node)
    {
        if (node is not ExpressionStatementSyntax ess) return node;
        var child = ess.ChildNodes().Single();
        if (child is not AssignmentExpressionSyntax assignment) return node;
        if (assignment.Left is not IdentifierNameSyntax identifierName) return node;
        var value = assignment.Right;
        var variableDeclarator = VariableDeclarator(identifierName.Identifier)
            .WithInitializer(EqualsValueClause(value));
        var variableDeclaration = VariableDeclaration(IdentifierName("var"))
            .WithVariables(SingletonSeparatedList(variableDeclarator));
        return LocalDeclarationStatement(variableDeclaration);        
    }
    private LocalDeclarationStatementSyntax ToLocalDeclaration(FieldDeclarationSyntax node)
    {
        var type = node.Declaration.Type;

        var implicitObjectCreationExpressions =
            from v in node.Declaration.Variables
            where v.Initializer is not null && v.Initializer.Value is ImplicitObjectCreationExpressionSyntax
            select (ImplicitObjectCreationExpressionSyntax)v.Initializer!.Value;

        foreach (var n in implicitObjectCreationExpressions)
        {
            node = node.ReplaceNode(n, ObjectCreationExpression(type).WithArgumentList(n.ArgumentList));
        }

        return LocalDeclarationStatement(
                            VariableDeclaration(type
                                ).WithVariables(
                                    SingletonSeparatedList(
                                        node.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single()
                                    )
                                )
                            );
    }


    private ClassDeclarationSyntax AdjustInputReading(ClassDeclarationSyntax aocclass)
    {
        var invocations = from i in aocclass.DescendantNodes().OfType<InvocationExpressionSyntax>()
                          where i.Expression is MemberAccessExpressionSyntax
                          let m = (MemberAccessExpressionSyntax)i.Expression
                          where m.Expression is IdentifierNameSyntax
                          let l = (IdentifierNameSyntax)m.Expression
                          where l.Identifier.Value is "Read"
                          let r = m.Name.Identifier.Value
                          where r is "InputLines" or "InputText" or "InputStream" or "SampleText" or "SampleLines" or "SampleStream"
                          select (i, r);

        foreach (var (invocation, right) in invocations)
        {
            aocclass = aocclass
                .ReplaceNode(invocation, InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("File"),
                        IdentifierName(right switch
                        {
                            "InputText" or "SampleText" => "ReadAllText",
                            "InputLines" or "SampleLines" => "ReadAllLines",
                            "InputStream" or "SampleStream" => "OpenRead",
                            _ => throw new NotSupportedException("Can not convert this call")
                        })
                        )
                    )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(right switch
                                    {
                                        "InputText" or "InputLines" or "InputStream" => "input.txt",
                                        "SampleText" or "SampleLines" or "SampleStream" => "sample.txt",
                                        _ => throw new NotSupportedException("Can not convert this call")
                                    })
                                    )
                                )
                            )
                        )
                    )
                );

        }
        return aocclass;
    }

    private static LocalFunctionStatementSyntax TransformToLocalFunctionStatement(MethodDeclarationSyntax method)
    {
        return LocalFunctionStatement(
            TokenList(method.Modifiers.Where(m => !m.IsKind(SyntaxKind.PublicKeyword))),
            method.ReturnType,
            method.Identifier,
            method.TypeParameterList,
            method.ParameterList,
            method.ConstraintClauses,
            method.Body,
            method.ExpressionBody
            );
    }

    private static MemberDeclarationSyntax GenerateGlobalStatement(int part, IReadOnlyDictionary<string, ExpressionSyntax> implementations)
    {
        return GlobalStatement(GenerateStatement(part, implementations));
    }
    private static StatementSyntax GenerateStatement(int part, IReadOnlyDictionary<string, ExpressionSyntax> implementations)
    {
        return implementations.ContainsKey($"Part{part}")
                        ? LocalDeclarationStatement(
                                VariableDeclaration(
                                    IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList()))
                                    )
                                .WithVariables(
                                    SingletonSeparatedList(VariableDeclarator(Identifier($"part{part}"))
                                    .WithInitializer(
                                            EqualsValueClause(
                                                implementations[$"Part{part}"]
                                                ))
                                        )
                                    )
                                )
                        : ParseStatement($"var part{part} = Part{part}();\r\n")!;
    }


    public async Task ExportCode(PuzzleKey key, string code, string[]? includecommon, string output)
    {
        var codeDir = fileSystem.GetCodeFolder(key);
        var commonDir = fileSystem.GetFolder("Common");
        var outputDir = fileSystem.GetOutputFolder(output);
        var templateDir = fileSystem.GetTemplateFolder(null);
        await outputDir.CreateIfNotExists();
        await outputDir.WriteCode(code);
        outputDir.CopyFiles(
            codeDir.GetCodeFiles().Where(f => !f.Name.EndsWith("Tests.cs"))
            );
        if (codeDir.Input.Exists)
            outputDir.CopyFile(codeDir.Input);

        if (codeDir.Sample.Exists)
            outputDir.CopyFile(codeDir.Sample);

        outputDir.CopyFile(templateDir.CsProj);

        if (includecommon is { Length: >0 } && commonDir.Exists)
        {
            await outputDir.CreateIfNotExists("Common");
            foreach (var name in includecommon)
            {
                foreach (var file in commonDir.GetFiles(Path.ChangeExtension(name, "cs")))
                {
                    outputDir.CopyFile(file, "Common");
                }
            }
        }
    }
}