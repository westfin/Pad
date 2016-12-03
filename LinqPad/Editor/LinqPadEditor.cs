using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Diagnostics;

namespace LinqPad.Editor
{
    public sealed class LinqPadEditor : TextEditor
    {
        private LinqPadCompletionWindow completionWindow;
        private IntellisenseProvider    intellisenseProvider;

        public IntellisenseProvider IntellisenseProvider
        {
            get { return intellisenseProvider; }
            set
            {
                if (intellisenseProvider == value)
                    return;
                intellisenseProvider = value;
            }
        }

        public LinqPadEditor() : base()
        {
            Options = new TextEditorOptions()
            {
                CutCopyWholeLine = true,
                ConvertTabsToSpaces = true,
                HighlightCurrentLine = true,
                IndentationSize = 4,
                EnableEmailHyperlinks = true
            };
            ShowLineNumbers = true;
            Attach();
        }

        private void Attach()
        {
            TextArea.TextEntered  += TextArea_TextEntered;
            TextArea.TextEntering += TextArea_TextEntering;
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            var results = await intellisenseProvider.GetCompletioData(CaretOffset);
            if (results != null && results.Count !=0)
            {
                completionWindow = new LinqPadCompletionWindow(TextArea);
                completionWindow.Closed += (o, args) => { completionWindow = null; };
                foreach (var item in results)
                {
                    completionWindow.CompletionList.CompletionData.Add(item);
                }
                completionWindow.Show();
            }
        }

        private sealed class LinqPadCompletionWindow : CompletionWindow
        {
            public LinqPadCompletionWindow(TextArea textarea) : base(textarea) { }
            static LinqPadCompletionWindow()
            {
                WindowStyleProperty.OverrideMetadata(typeof(LinqPadCompletionWindow),
                    new FrameworkPropertyMetadata(WindowStyle.None));

                AllowsTransparencyProperty.OverrideMetadata(typeof(LinqPadCompletionWindow),
                    new FrameworkPropertyMetadata(true));
            }
        }
    }
}
