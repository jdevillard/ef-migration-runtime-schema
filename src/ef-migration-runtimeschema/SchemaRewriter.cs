using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace JDEV.EFMigrationRuntimeSchema
{
    public class SchemaRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitArgument(ArgumentSyntax node)
        {
            //Replace Schema for Up and Down
            if (node.NameColon?.Name.Identifier.Text == "schema")
                return CreateSchemaRefnode(node);


            //Replace Schema Name for migrationBuilder.EnsureSchema()
            if (node.NameColon?.Name.Identifier.Text == "name" 
                && node.Parent != null
                && node.Parent.Parent.IsKind(SyntaxKind.InvocationExpression))
            {
                var methodNode = node.Parent.Parent.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().First();
                var nameMethod = (methodNode.Expression as MemberAccessExpressionSyntax)?.Name;

                if(nameMethod?.Identifier.Text =="EnsureSchema")
                    return CreateSchemaRefnode(node);
            }
            
            return base.VisitArgument(node);
        }

        private static SyntaxNode CreateSchemaRefnode(ArgumentSyntax node)
        {
            return SyntaxFactory.Argument(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName("_schema"),
                                                                            SyntaxFactory.IdentifierName("Schema")))
                                .WithNameColon(node.NameColon);
        }

    }
}