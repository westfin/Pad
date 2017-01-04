using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

using Microsoft.CodeAnalysis.Text;

using TextChangeEventArgs = Microsoft.CodeAnalysis.Text.TextChangeEventArgs;

namespace LinqPad.Editor
{
    public class LinqPadSourceTextContainer : SourceTextContainer
    {
        private readonly TextEditor textEditor;

        private SourceText currentSourceText;

        public LinqPadSourceTextContainer(TextEditor textEditor)
        {
            this.textEditor = textEditor;
            this.currentSourceText = SourceText.From(textEditor.Text);
            this.textEditor.Document.Changed += this.DocumentOnChanged;
        }

        public override event EventHandler<TextChangeEventArgs> TextChanged;

        public TextDocument Document => this.textEditor.Document;

        public override SourceText CurrentText => this.currentSourceText;

        private void DocumentOnChanged(object sender, DocumentChangeEventArgs e)
        {
            var oldSourceText = this.currentSourceText;

            var textSpan = new TextSpan(e.Offset, e.RemovalLength);
            var textChangeRange = new TextChangeRange(textSpan, e.InsertionLength);
            this.currentSourceText = this.currentSourceText.WithChanges(new TextChange(textSpan, e.InsertedText?.Text ?? string.Empty));

            this.TextChanged?.Invoke(this, new TextChangeEventArgs(oldSourceText, this.currentSourceText, textChangeRange));
        }
    }
}
