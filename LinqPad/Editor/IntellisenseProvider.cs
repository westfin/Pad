using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ICSharpCode.AvalonEdit;
using Microsoft.CodeAnalysis.Completion;
using System.Diagnostics;

namespace LinqPad.Editor
{
    public sealed class IntellisenseProvider
    {
        private readonly RoslynEditorHost roslynHost;
        private readonly DocumentId documentId;
        public IntellisenseProvider(TextEditor editor, RoslynEditorHost roslynHost, DocumentId documentId)
        {
            this.roslynHost = roslynHost;
            this.documentId = documentId;
        }
        public async Task<IList<ICompletionData>> GetCompletioData(int position)
        {
            List<ICompletionData> result = null;
            var document = roslynHost.GetDocument(documentId);
            var completionService = CompletionService.GetService(document);
            var data = await completionService.GetCompletionsAsync(
                document,
                position,
                CompletionTrigger.CreateInsertionTrigger('.'));

            if (data != null)
            {
                result = data.Items.Select(i => new CompletionData(i, document)).
                    ToList<ICompletionData>();
            }
            return result;
        }
    }
}
