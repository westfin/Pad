using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

using Microsoft.CodeAnalysis;

namespace LinqPad.Editor
{
    public sealed class LinqPadHightlighter : IHighlighter
    {
        private readonly DocumentId documentId;

        public LinqPadHightlighter(IDocument document, DocumentId documentId)
        {
            this.Document = document;
            this.documentId = documentId;
        }

        public event HighlightingStateChangedEventHandler HighlightingStateChanged;

        public HighlightingColor DefaultTextColor
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDocument Document { get; }

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
