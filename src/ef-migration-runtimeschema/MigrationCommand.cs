// See https://aka.ms/new-console-template for more information
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.Serialization;
using Formatter = Microsoft.CodeAnalysis.Formatting.Formatter;

namespace JDEV.EFMigrationRuntimeSchema
{
    public class MigrationCommand()
    {
        public void Execute(string interfaceName, string migrationFilePath)
        {
            // creation of the syntax tree for every file
            var programPath = migrationFilePath;
            var programText = File.ReadAllText(programPath);
            SyntaxNode result = RewriteSyntaxtNode(interfaceName, programPath, programText);

            using var fileWriter = new StreamWriter(programPath, append: false);
            result.WriteTo(fileWriter);
        }

        public static SyntaxNode RewriteSyntaxtNode(string interfaceName, string programPath, string programText)
        {
            SyntaxTree programTree = CSharpSyntaxTree.ParseText(programText)
                                                 .WithFilePath(programPath);

            var root = (CompilationUnitSyntax)programTree.GetRoot();

            // Get the first class from the syntax tree
            var migrationClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var className = migrationClass.Identifier;
            var schemaInterface = interfaceName;

            var ctor = SyntaxFactory.ConstructorDeclaration(className)
                  .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                  .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("schema"))
                        .WithType(SyntaxFactory.ParseTypeName(schemaInterface)))
                  .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement($"_schema = schema;"))

                  );

            var field = SyntaxFactory.FieldDeclaration(
                                    SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.IdentifierName(schemaInterface))
                                    .WithVariables(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier("_schema")))))
                                .WithModifiers(
                                    SyntaxFactory.TokenList(
                                        [SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)]
                                    ));

            var oldMembers = migrationClass.Members;

            if (!migrationClass.Members.Any(m => m is ConstructorDeclarationSyntax))
            {
                oldMembers = oldMembers.Insert(0, ctor
                    .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia("/// <inheritdoc />\n"))
                    .WithAdditionalAnnotations(Formatter.Annotation));
            }

            if (!migrationClass.Members.Any(m => m is FieldDeclarationSyntax field
                && field.Declaration.Variables.Any(v => v.Identifier.ValueText == "_schema"))
                )
            {
                oldMembers = oldMembers.Insert(0, field.WithAdditionalAnnotations(Formatter.Annotation));
            }


            var updatedClass = migrationClass.AddMembers(ctor);
            updatedClass = migrationClass.WithMembers(oldMembers);
            var updatedRoot = root.ReplaceNode(migrationClass, updatedClass);
            var formattedRoot = Formatter.Format(updatedRoot, Formatter.Annotation, new AdhocWorkspace());

            var schemaRewriter = new SchemaRewriter();
            var result = schemaRewriter.Visit(formattedRoot);
            return result;
        }
    }
}