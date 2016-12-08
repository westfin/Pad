using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;

using TextDocument = ICSharpCode.AvalonEdit.Document.TextDocument;
using Microsoft.CodeAnalysis;

namespace LinqPad.Editor
{
    public sealed class LinqPadHightlighter : IHighlighter
    {
        private readonly IDocument document;
        private readonly DocumentId documentId;

        public LinqPadHightlighter(IDocument document, DocumentId documentId)
        {
            this.document = document;
            this.documentId = documentId;
        }

        public HighlightingColor DefaultTextColor
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDocument Document => document;

        public event HighlightingStateChangedEventHandler HighlightingStateChanged;

        public void BeginHighlighting()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EndHighlighting()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<HighlightingColor> GetColorStack(int lineNumber)
        {
            throw new NotImplementedException();
        }

        public HighlightingColor GetNamedColor(string name)
        {
            throw new NotImplementedException();
        }

        public HighlightedLine HighlightLine(int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void UpdateHighlightingState(int lineNumber)
        {
            throw new NotImplementedException();
        }
    }
}
