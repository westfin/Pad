using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextChangeEventArgs = Microsoft.CodeAnalysis.Text.TextChangeEventArgs;

namespace LinqPad.Editor
{
    public class LinqPadSourceTextContainer : SourceTextContainer
    {
        private readonly TextEditor textEditor;
        private SourceText currentSourceText;
        public  TextDocument Document => textEditor.Document;

        public LinqPadSourceTextContainer(TextEditor textEditor)
        {
            this.textEditor = textEditor;
            currentSourceText = SourceText.From(textEditor.Text);
            textEditor.Document.Changed += DocumentOnChanged;
        }

        public override SourceText CurrentText => currentSourceText;

        private void DocumentOnChanged(object sender, DocumentChangeEventArgs e)
        {
            var oldSourceText = currentSourceText;

            var textSpan = new TextSpan(e.Offset, e.RemovalLength);
            var textChangeRange = new TextChangeRange(textSpan, e.InsertionLength);
            currentSourceText = currentSourceText.WithChanges(new TextChange(textSpan, e.InsertedText?.Text ?? string.Empty));

            TextChanged?.Invoke(this, new TextChangeEventArgs(oldSourceText, currentSourceText, textChangeRange));
        }

        public override event EventHandler<TextChangeEventArgs> TextChanged;
    }
}
