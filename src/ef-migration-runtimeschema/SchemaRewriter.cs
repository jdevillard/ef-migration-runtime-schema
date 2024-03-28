using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JDEV.EFMigrationRuntimeSchema
{
    public class SchemaRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitArgument(ArgumentSyntax node)
        {
            if (node.NameColon?.Name.Identifier.Text == "schema")
                return SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName("_schema"),
                                                                SyntaxFactory.IdentifierName("Schema")))
                    .WithNameColon(node.NameColon);

            return base.VisitArgument(node);
        }
    }
}