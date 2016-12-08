using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.CodeAnalysis;

using TextDocument = ICSharpCode.AvalonEdit.Document.TextDocument;

namespace LinqPad.Editor
{
    public sealed class LinqPadHighlightingColorizer : HighlightingColorizer
    {
        private readonly DocumentId documentId;

        public LinqPadHighlightingColorizer(DocumentId documentId)
        {
            this.documentId = documentId;
        }

        protected override IHighlighter CreateHighlighter(TextView textView, TextDocument document)
        {
            return new LinqPadHightlighter(document, documentId);
        }
    }
}
