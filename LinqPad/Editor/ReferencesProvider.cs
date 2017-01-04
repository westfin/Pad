using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.Editor
{
    public sealed class ReferencesProvider
    {
        private readonly LinqPadEditorHost host;
        public ReferencesProvider(LinqPadEditorHost host)
        {
            this.host = host;
        }

        public async Task<IEnumerable<string>> GetReferences(DocumentId documentId)
        {
            var tree = await host.GetDocument(documentId).GetSyntaxTreeAsync();
            var root = await tree.GetRootAsync() as CompilationUnitSyntax;

            var directives = root?.GetReferenceDirectives();
            if (directives != null)
                return !(directives.Count > 0) ? null : directives.Select(i => i.File.ValueText);
            return null;
        }
    }
}
