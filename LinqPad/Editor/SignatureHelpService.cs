using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace LinqPad.Editor
{
    public sealed class SignatureHelpService
    {
        private static readonly SymbolDisplayFormat SymbolDisplayFormat =
            DefaultSymbolDisplayFormat();

        private readonly LinqPadEditorHost host;

        private readonly DocumentId documentId;

        public SignatureHelpService(LinqPadEditorHost host, DocumentId documentId)
        {
            this.host = host;
            this.documentId = documentId;
        }

        public async Task<SignatureHelp> GetSignatureHelp(int position)
        {
            var document   = this.host.GetDocument(this.documentId);
            var signature  = new SignatureHelp();
            var invocation = await GetInvocation(document, position).ConfigureAwait(false);

            if (invocation == null)
            {
                return null;
            }

            var methods = GetMethodOverloads(invocation.SemanticModel, invocation.Node);
            if (methods == null)
            {
                return null;
            }

            foreach (var comma in invocation.ArgumentList.Arguments.GetSeparators())
            {
                if (comma.Span.Start > invocation.Position)
                {
                    break;
                }

                signature.ActiveParameter += 1;
            }

            var signatureHelpItemList = new List<SignatureHelpItem>();

            var bestScore = int.MinValue;
            SignatureHelpItem bestScoredItem = null;

            var types = invocation.ArgumentList.Arguments
                    .Select(argument => invocation.SemanticModel.GetTypeInfo(argument.Expression));

            foreach (var method in methods)
            {
                var buildItem = BuildItem(method, invocation.SemanticModel, position);
                signatureHelpItemList.Add(buildItem);
                var score = InvocationScore(method, types);
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestScoredItem = buildItem;
            }

            if (bestScoredItem == null)
            {
                return null;
            }

            signature.ActiveSignature = signatureHelpItemList.IndexOf(bestScoredItem);
            signature.SignatureHelpItems = signatureHelpItemList;
            return signature;
        }

        private static async Task<InvocationContext> GetInvocation(Document document, int position)
        {
            var tree = await document.GetSyntaxTreeAsync();
            var root = await tree.GetRootAsync();

            var token = root.FindToken(position).IsKind(SyntaxKind.EndOfFileToken) ?
                root.FindToken(position).GetPreviousToken() : root.FindToken(position);
            var node  = token.Parent;
            while (node != null)
            {
                var invocation = node as InvocationExpressionSyntax;
                if (invocation != null && invocation.ArgumentList.Span.Contains(position - 1))
                {
                    return new InvocationContext()
                    {
                        SemanticModel = await document.GetSemanticModelAsync(),
                        Position = position,
                        Node = invocation.Expression,
                        ArgumentList = invocation.ArgumentList
                    };
                }

                var objectCreation = node as ObjectCreationExpressionSyntax;
                if (objectCreation?.ArgumentList != null)
                {
                    if (objectCreation.Span.Contains(position - 1))
                    {
                        return new InvocationContext()
                        {
                            SemanticModel = await document.GetSemanticModelAsync(),
                            Position = position,
                            Node = objectCreation,
                            ArgumentList = objectCreation.ArgumentList
                        };
                    }
                }

                node = node.Parent;
            }

            return null;
        }

        private static int InvocationScore(IMethodSymbol symbol, IEnumerable<TypeInfo> types)
        {
            var parameters = GetParameters(symbol);
            if (parameters.Count() < types.Count())
            {
                return int.MinValue;
            }

            var score = 0;
            var invocationEnum = types.GetEnumerator();
            var definitionEnum = parameters.GetEnumerator();
            while (invocationEnum.MoveNext() && definitionEnum.MoveNext())
            {
                if (invocationEnum.Current.ConvertedType == null)
                {
                    // 1 point for having a parameter
                    score += 1;
                }
                else if (invocationEnum.Current.ConvertedType.Equals(definitionEnum.Current.Type))
                {
                    // 2 points for having a parameter and being
                    // the same type
                    score += 2;
                }
            }

            return score;
        }

        private static IEnumerable<IMethodSymbol> GetMethodOverloads(SemanticModel model, SyntaxNode node)
        {
            ISymbol symbol = null;
            var symbolInfo = model.GetSymbolInfo(node);
            if (symbolInfo.Symbol != null)
            {
                symbol = symbolInfo.Symbol;
            }
            else if (!symbolInfo.CandidateSymbols.IsEmpty)
            {
                symbol = symbolInfo.CandidateSymbols.First();
            }

            return symbol?.ContainingType == null ? new IMethodSymbol[] { } :
                symbol.ContainingType.GetMembers(symbol.Name).OfType<IMethodSymbol>();
        }
        
        private static IEnumerable<IParameterSymbol> GetParameters(IMethodSymbol method)
        {
            return !method.IsExtensionMethod ? method.Parameters : method.Parameters.RemoveAt(0);
        }
        
        private static SignatureHelpItem BuildItem(IMethodSymbol method, SemanticModel model, int position)
        {
            var item = new SignatureHelpItem
            {
                Name  = method.MethodKind == MethodKind.Constructor ? method.ContainingType.Name : method.Name,
                Lable = method.ToMinimalDisplayParts(model, position, SymbolDisplayFormat),
                Documentation = method.GetDocumentationCommentXml(),
                Parametrs     = GetParameters(method).Select(i => new SignatureHelpParametr()
                {
                    Name  = i.Name,
                    Lable = i.ToMinimalDisplayParts(model, position),
                    Documentation = i.GetDocumentationCommentXml()
                })
            };

            return item;
        }

        private static SymbolDisplayFormat DefaultSymbolDisplayFormat()
        {
            return new SymbolDisplayFormat(
                    memberOptions:
                        SymbolDisplayMemberOptions.IncludeParameters |
                        SymbolDisplayMemberOptions.IncludeContainingType |
                        SymbolDisplayMemberOptions.IncludeType |
                        SymbolDisplayMemberOptions.IncludeExplicitInterface,
                    parameterOptions:
                        SymbolDisplayParameterOptions.IncludeOptionalBrackets |
                        SymbolDisplayParameterOptions.IncludeDefaultValue |
                        SymbolDisplayParameterOptions.IncludeParamsRefOut |
                        SymbolDisplayParameterOptions.IncludeType |
                        SymbolDisplayParameterOptions.IncludeName,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes,
                    extensionMethodStyle: SymbolDisplayExtensionMethodStyle.InstanceMethod);
        } 
    }

    public sealed class InvocationContext
    {
        public SemanticModel SemanticModel { get; set; }

        public int Position { get; set; }

        public SyntaxNode Node { get; set; }

        public ArgumentListSyntax ArgumentList { get; set; }
    }

    public sealed class SignatureHelp
    {
        public List<SignatureHelpItem> SignatureHelpItems { get; set; }

        public int ActiveSignature { get; set; }

        public int ActiveParameter { get; set; }
    }

    public sealed class SignatureHelpItem
    {
        public string Name { get; set; }

        public IEnumerable<SymbolDisplayPart> Lable { get; set; }

        public string Documentation { get; set; }

        public IEnumerable<SignatureHelpParametr> Parametrs { get; set; }
    }

    public sealed class SignatureHelpParametr
    {
        public string Name { get; set; }

        public IEnumerable<SymbolDisplayPart> Lable { get; set; }

        public string Documentation { get; set; }
    }
}
