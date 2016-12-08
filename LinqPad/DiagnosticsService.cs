using LinqPad.Editor;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad
{
    public sealed class DiagnosticsService
    {
        private readonly RoslynEditorHost host;
        public DiagnosticsService(RoslynEditorHost host)
        {
            this.host = host;
        }

        public async Task<List<Diagnostic>> GetDiagnostics(DocumentId documentId)
        {
            var model = await host.GetDocument(documentId).GetSemanticModelAsync();
            var diagnostics = model.GetDiagnostics();
            return diagnostics.ToList();
        }
    }
}
