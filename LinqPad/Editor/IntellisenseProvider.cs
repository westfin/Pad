using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit.CodeCompletion;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace LinqPad.Editor
{
    public sealed class IntellisenseProvider
    {
        private readonly LinqPadEditorHost roslynHost;

        private readonly DocumentId documentId;

        public IntellisenseProvider(LinqPadEditorHost roslynHost, DocumentId documentId)
        {
            this.roslynHost = roslynHost;
            this.documentId = documentId;
        }

        public async Task<IList<ICompletionData>> GetCompletioData(int position, char trieggerChar)
        {
            IList<ICompletionData> result = null;
            var document = this.roslynHost.GetDocument(this.documentId);
            var completionService = CompletionService.GetService(document);

            var data = await completionService.GetCompletionsAsync(
                document: document,
                caretPosition: position,
                trigger: GetCompletionTrigger(trieggerChar)).ConfigureAwait(false);

            if (data != null && data.Items.Any())
            {
                result = data.Items.Select(i => new CompletionData(i, document)).ToList<ICompletionData>();
            }
            else
            {
                result = Array.Empty<ICompletionData>();
            }

            return result;
        }

        private static CompletionTrigger GetCompletionTrigger(char? triggerChar)
        {
            return triggerChar != null
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                : CompletionTrigger.Default;
        }
    }
}
