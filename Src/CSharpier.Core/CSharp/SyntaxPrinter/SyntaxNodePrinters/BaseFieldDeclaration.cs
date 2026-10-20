using CSharpier.Core.DocTypes;
using CSharpier.Core.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core.CSharp.SyntaxPrinter.SyntaxNodePrinters;

internal static class BaseFieldDeclaration
{
    public static Doc Print(BaseFieldDeclarationSyntax node, CSharpPrintingContext context)
    {
        var canSameLine =
            context.FormattingStyle == FormattingStyle.Resharper
            && node.AttributeLists.Count > 0
            && AttributeLists.GetTotalAttributeCount(node.AttributeLists) <= 2
            && !AttributeLists.HasOriginalLineBreaks(node.AttributeLists);

        using var docs = new DocListBuilder(5);
        docs.Add(AttributeLists.Print(node, node.AttributeLists, context));
        docs.Add(Modifiers.PrintSorted(node.Modifiers, context));
        if (node is EventFieldDeclarationSyntax eventFieldDeclarationSyntax)
        {
            docs.Add(Token.PrintWithSuffix(eventFieldDeclarationSyntax.EventKeyword, " ", context));
        }

        docs.Add(
            VariableDeclaration.Print(node.Declaration, context),
            Token.Print(node.SemicolonToken, context)
        );

        if (canSameLine)
        {
            return Doc.Group(docs.ToArray());
        }

        return Doc.Concat(docs);
    }
}
