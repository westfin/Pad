using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Media;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Windows;

namespace LinqPad.Editor
{
    public sealed class CompletionData : ICompletionData
    {
        private Document document;
        private CompletionItem completionItem;
        public CompletionData(CompletionItem item, Document document)
        {
            var key = item.GetGlyph();
            this.Image = Application.Current.FindResource(key) as ImageSource;
            this.completionItem = item;
            this.document = document;
            this.Text = item.DisplayText;
            this.Content = item.DisplayText;
        }
        public object Content       { get; }
        public object Description   { get; }
        public ImageSource Image    { get; }
        public double Priority      { get; }
        public string Text          { get; }

        public async void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var completionChanges = await CompletionService.GetService(document)
                .GetChangeAsync(document, completionItem).ConfigureAwait(false);

            var change = completionChanges.TextChange;
            var textDocument = textArea.Document;
            using (textDocument.RunUpdate())
            {
                if (completionSegment.EndOffset > change.Span.End)
                {
                    textDocument.Replace(
                        new TextSegment { StartOffset = change.Span.End, EndOffset = completionSegment.EndOffset },
                        string.Empty);
                }

                textDocument.Replace(change.Span.Start, change.Span.Length,
                    new StringTextSource(change.NewText));
            }
        }
    }
}
