using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

using Net.Code.AdventOfCode.Toolkit.Core;

using System.Diagnostics;
using System.Reflection;

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

        if (codeFolder.Exists && !force)
        {
            throw new Exception($"Puzzle for {year}/{day} already initialized. Use --force to re-initialize.");
        }

        var input = await client.GetPuzzleInputAsync(year, day);
        await client.GetPuzzleAsync(year, day, !force);

        await codeFolder.CreateIfNotExists();

        var code = await templateDir.ReadCode(year, day);
        await codeFolder.WriteCode(code);
        await codeFolder.WriteSample("");
        await codeFolder.WriteInput(input);
        if (templateDir.Notebook.Exists)
        {
            codeFolder.CopyFile(templateDir.Notebook);
        }
    }

    public async Task<string> GenerateCodeAsync(int year, int day)
    {
        var dir = fileSystem.GetCodeFolder(year, day);
        var aoc = await dir.ReadCode();
        var tree = CSharpSyntaxTree.ParseText(aoc);

        tree = tree.WithRootAndOptions(ExtractRegexGenerators(tree.GetRoot()), tree.Options);

        // TODO could be used to selectively add methods from Common
        // var common = fileSystem.GetFolder("Common");
        // var additionalTrees = (
        //    from f in common.GetFiles("*.cs")
        //    let text = common.ReadFile(f.FullName).Result
        //    select CSharpSyntaxTree.ParseText(text)).ToArray();


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
            let fieldname = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().Identifier.ToString()
            select ToLocalDeclaration(node);

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
                List(
                    fields.Select(GlobalStatement)
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
                            VariableDeclaration(
                                IdentifierName(
                                    Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())
                                    )
                                ).WithVariables(
                                    SingletonSeparatedList(
                                        node.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single()
                                    )
                                )
                            );
    }

    private SyntaxNode ExtractRegexGenerators(SyntaxNode root)
    {
        var regexgenerators = from m in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                              where m.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                              where (from al in m.AttributeLists
                                     from a in al.Attributes
                                     where a.Name.ToString() is "GeneratedRegex"
                                     select m).Any()
                              select m;


        var regexAsCall = (
            from m in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
            where m.Expression is MemberAccessExpressionSyntax
            let exp = (MemberAccessExpressionSyntax)m.Expression
            where exp.Name is GenericNameSyntax
            let name = (GenericNameSyntax)exp.Name
            where name.Identifier.ValueText is "As"
            && name.TypeArgumentList.Arguments.Count == 1
            select m
            ).Any();

        if (regexgenerators.Any() || regexAsCall)
        {
            var regexinvocations = from m in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                   where m.Expression is IdentifierNameSyntax
                                   let x = (IdentifierNameSyntax)m.Expression
                                   join rex in regexgenerators on x.Identifier.Value equals rex.Identifier.Value
                                   select (m, x);

            foreach (var (node, identifier) in regexinvocations)
            {
                root = root.ReplaceNode(node, InvocationExpression(
                         MemberAccessExpression(
                         SyntaxKind.SimpleMemberAccessExpression,
                         IdentifierName("AoCRegex"),
                         IdentifierName(identifier.Identifier.ValueText)
                         ))
                     );
            }

            var c = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Last();
            root = root.InsertNodesAfter(c, List(
                        new[]
                        {
                            ClassDeclaration("AoCRegex")
                            .WithModifiers(TokenList(
                                Token(SyntaxKind.StaticKeyword),
                                Token(SyntaxKind.PartialKeyword)
                                ))
                            .WithMembers(
                                List(regexgenerators.Select(m => m.WithModifiers(TokenList(
                                    Token(SyntaxKind.PublicKeyword),
                                    Token(SyntaxKind.StaticKeyword),
                                    Token(SyntaxKind.PartialKeyword)
                                    )) as MemberDeclarationSyntax)
                                .Concat(new[]{
                                ParseMemberDeclaration(
                                """
                                    public static T As<T>(this Regex regex, string s, IFormatProvider? provider = null) where T: struct
                                    {
                                        var match = regex.Match(s);
                                        if (!match.Success) throw new InvalidOperationException($"input '{s}' does not match regex '{regex}'");

                                        var constructor = typeof(T).GetConstructors().Single();
        
                                        var j = from p in constructor.GetParameters()
                                                join m in match.Groups.OfType<Group>() on p.Name equals m.Name
                                                select Convert.ChangeType(m.Value, p.ParameterType, provider ?? CultureInfo.InvariantCulture);

                                        return (T)constructor.Invoke(j.ToArray());

                                    }
                                """)!, ParseMemberDeclaration(
                                """
                                    public static int GetInt32(this Match m, string name) => int.Parse(m.Groups[name].Value);
                                """)!
                            })
                            ))
                        }));

        }


        return root;
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


    public async Task ExportCode(int year, int day, string code, bool includecommon, string output)
    {
        var codeDir = fileSystem.GetCodeFolder(year, day);
        var commonDir = fileSystem.GetFolder("Common");
        var outputDir = fileSystem.GetOutputFolder(output);
        var templateDir = fileSystem.GetTemplateFolder();
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
        if (includecommon && commonDir.Exists)
            outputDir.CopyFiles(commonDir.GetFiles("*.cs"));
    }
}