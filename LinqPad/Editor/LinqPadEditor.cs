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
using System.Windows.Controls;

namespace LinqPad.Editor
{
    public sealed class LinqPadEditor : TextEditor
    {
        private LinqPadCompletionWindow completionWindow;
        private OverloadInsightWindow   insightWindow;
        private IntellisenseProvider    intellisenseProvider;
        private SignatureHelpService    signatureHelpService;
        private ToolTip toolTip;
        public Action<ToolTipArgs> ToolTipRequest { get; set; }


        public static readonly DependencyProperty CompletionBackgroundProperty = DependencyProperty.Register(
            "CompletionBackground", typeof(Brush), typeof(LinqPadEditor), new FrameworkPropertyMetadata(CreateDefaultCompletionBackground()));

        private static SolidColorBrush CreateDefaultCompletionBackground()
        {
            var defaultCompletionBackground = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            defaultCompletionBackground.Freeze();
            return defaultCompletionBackground;
        }

        public Brush CompletionBackground
        {
            get { return (Brush)GetValue(CompletionBackgroundProperty); }
            set { SetValue(CompletionBackgroundProperty, value); }
        }

        public SignatureHelpService SignatureHelpService
        {
            get { return signatureHelpService; }
            set
            {
                if (signatureHelpService == value)
                    return;
                signatureHelpService = value;
            }
        }

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

        private void LinqPadEditor_MouseHover(object sender, MouseEventArgs e)
        {
            TextViewPosition? position;
            try
            {
                position = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
            }
            catch (ArgumentOutOfRangeException)
            {
                e.Handled = true;
                return;
            }
            var args = new ToolTipArgs() { InDocument = position.HasValue };

            if (!position.HasValue || position.Value.Location.IsEmpty)
            {
                return;
            }
            args.LogicalPosition = position.Value.Location;
            args.Position = Document.GetOffset(position.Value.Line, position.Value.Column);

            if (ToolTipRequest == null)
                return;

            ToolTipRequest.Invoke(args);
            if (args.ContentToShow == null)
                return;

            if(toolTip == null)
            {
                toolTip = new ToolTip { MaxWidth = 400 };
                toolTip.Closed += delegate { toolTip = null; };
            }

            var stringContent = args.ContentToShow as string;
            if (stringContent != null)
            {
                toolTip.Content = new TextBlock
                {
                    Text = stringContent,
                    TextWrapping = TextWrapping.Wrap
                };
            }
            else
            {
                toolTip.Content = args.ContentToShow;
            }

            e.Handled = true;
            toolTip.IsOpen = true;
        }

        private void OnVisualLinesChanged(object sender, EventArgs e)
        {
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
            }
        }

        private void OnMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
                e.Handled = true;
            }
        }

        private void Attach()
        {
            TextArea.TextEntered  += TextArea_TextEntered;
            TextArea.TextEntering += TextArea_TextEntering;
            MouseHover            += LinqPadEditor_MouseHover;
            MouseHoverStopped     += OnMouseHoverStopped;
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            //if (e.Text.Length > 0 && completionWindow != null)
            //{
            //    if (!char.IsLetterOrDigit(e.Text[0]))
            //    {
            //        // Whenever a non-letter is typed while the completion window is open,
            //        // insert the currently selected element.
            //        completionWindow.CompletionList.RequestInsertion(e);
            //    }
            //}
        }

        private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            var help = await signatureHelpService.GetSignatureHelp(CaretOffset);
            if(help != null)
            {
                var provider = new OverloadProvider(help);
                if (insightWindow == null)
                {
                    insightWindow = new OverloadInsightWindow(TextArea);
                    insightWindow.Closed += delegate { insightWindow = null; };
                    insightWindow.Background = CompletionBackground;
                    insightWindow.Style = TryFindResource(typeof(InsightWindow)) as Style;
                }
                insightWindow.Provider = provider;
                insightWindow.Show();
                return;
            }

            insightWindow?.Close();
            var results = await intellisenseProvider.GetCompletioData(CaretOffset);
            if (results != null && results.Count !=0)
            {
                completionWindow = new LinqPadCompletionWindow(TextArea) { Background = CompletionBackground };
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
