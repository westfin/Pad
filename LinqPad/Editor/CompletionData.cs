using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;

namespace LinqPad.Editor
{
    public sealed class CompletionData : ICompletionData
    {
        private readonly Document document;

        private readonly CompletionItem completionItem;

        public CompletionData(CompletionItem item, Document document)
        {
            var key = item.GetGlyph();
            this.Image = Application.Current.FindResource(key) as ImageSource;
            this.completionItem = item;
            this.document = document;
            this.Text = item.DisplayText;
            this.Content = item.DisplayText;
        }

        public object Content { get; }

        public object Description { get; }

        public ImageSource Image { get; }

        public double Priority { get; }

        public string Text { get; }

        public async void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var completionChanges = await CompletionService.GetService(this.document)
                .GetChangeAsync(this.document, this.completionItem).ConfigureAwait(false);

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

                textDocument.Replace(change.Span.Start, change.Span.Length, new StringTextSource(change.NewText));
            }
        }
    }
}
