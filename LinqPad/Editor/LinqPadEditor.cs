using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;

namespace LinqPad.Editor
{
    public sealed class LinqPadEditor : TextEditor
    {
        public static readonly DependencyProperty CompletionBackgroundProperty = 
            DependencyProperty.Register(
                "CompletionBackground",
                typeof(Brush),
                typeof(LinqPadEditor),
                new FrameworkPropertyMetadata(CreateDefaultCompletionBackground()));

        private LinqPadCompletionWindow completionWindow;

        private OverloadInsightWindow insightWindow;

        private IntellisenseProvider intellisenseProvider;

        private SignatureHelpService signatureHelpService;

        private ToolTip toolTip;

        public LinqPadEditor() : base()
        {
            SearchPanel.Install(this);
            this.Options = new TextEditorOptions()
            {
                AllowScrollBelowDocument = true,
                CutCopyWholeLine = true,
                ConvertTabsToSpaces = true,
                HighlightCurrentLine = true,
                IndentationSize = 4,
                EnableEmailHyperlinks = true
            };
            this.ShowLineNumbers = true;
            this.Attach();
        }

        public Action<ToolTipArgs> ToolTipRequest { private get; set; }

        public Brush CompletionBackground
        {
            get { return (Brush)this.GetValue(CompletionBackgroundProperty); }
            set { this.SetValue(CompletionBackgroundProperty, value); }
        }

        public SignatureHelpService SignatureHelpService
        {
            get
            {
                return this.signatureHelpService;
            }

            set
            {
                if (this.signatureHelpService == value)
                {
                    return;
                }

                this.signatureHelpService = value;
            }
        }

        public IntellisenseProvider IntellisenseProvider
        {
            get
            {
                return this.intellisenseProvider;
            }

            set
            {
                if (this.intellisenseProvider == value)
                {
                    return;
                }

                this.intellisenseProvider = value;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                this.ShowCompletion().ConfigureAwait(true);
                e.Handled = true;
            }
        }

        private static SolidColorBrush CreateDefaultCompletionBackground()
        {
            var defaultCompletionBackground = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            defaultCompletionBackground.Freeze();
            return defaultCompletionBackground;
        }

        private void LinqPadEditorMouseHover(object sender, MouseEventArgs e)
        {
            TextViewPosition? position;
            try
            {
                position = this.TextArea.TextView.GetPositionFloor(e.GetPosition(this.TextArea.TextView) + this.TextArea.TextView.ScrollOffset);
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
            args.Position = this.Document.GetOffset(position.Value.Line, position.Value.Column);

            if (this.ToolTipRequest == null)
            {
                return;
            }

            this.ToolTipRequest.Invoke(args);
            if (args.ContentToShow == null)
            {
                return;
            }

            if (this.toolTip == null)
            {
                this.toolTip = new ToolTip { MaxWidth = 400 };
                this.toolTip.Closed += delegate { this.toolTip = null; };
            }

            var stringContent = args.ContentToShow as string;
            if (stringContent != null)
            {
                this.toolTip.Content = new TextBlock
                {
                    Text = stringContent,
                    TextWrapping = TextWrapping.Wrap
                };
            }
            else
            {
                this.toolTip.Content = args.ContentToShow;
            }

            e.Handled = true;
            this.toolTip.IsOpen = true;
        }

        private void OnVisualLinesChanged(object sender, EventArgs e)
        {
            if (this.toolTip != null)
            {
                this.toolTip.IsOpen = false;
            }
        }

        private void OnMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (this.toolTip == null)
            {
                return;
            }

            this.toolTip.IsOpen = false;
            e.Handled = true;
        }

        private void Attach()
        {
            this.TextArea.TextEntered  += this.TextAreaTextEntered;
            this.TextArea.TextEntering += this.TextAreaTextEntering;
            this.MouseHover            += this.LinqPadEditorMouseHover;
            this.MouseHoverStopped     += this.OnMouseHoverStopped;
        }

        private void TextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
        }

        private void TextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            this.ShowCompletion().ConfigureAwait(true);
        }

        private async Task ShowCompletion()
        {
            var help = await this.signatureHelpService.GetSignatureHelp(this.CaretOffset);
            if (help != null)
            {
                var provider = new OverloadProvider(help);
                if (this.insightWindow == null)
                {
                    this.insightWindow = new OverloadInsightWindow(this.TextArea);
                    this.insightWindow.Closed += delegate { this.insightWindow = null; };
                    this.insightWindow.Background = this.CompletionBackground;
                    this.insightWindow.Style = this.TryFindResource(typeof(InsightWindow)) as Style;
                }

                this.insightWindow.Provider = provider;
                this.insightWindow.Show();
                return;
            }

            this.insightWindow?.Close();
            var results = await this.intellisenseProvider.GetCompletioData(
                this.CaretOffset,
                this.Document.GetCharAt(this.CaretOffset - 1)).ConfigureAwait(true);

            if (results?.Any() == true && this.completionWindow == null)
            {
                this.completionWindow = new LinqPadCompletionWindow(this.TextArea)
                {
                    Background = this.CompletionBackground,
                    CloseWhenCaretAtBeginning = false,
                };

                this.completionWindow.CompletionList.IsFiltering = true;
                var data = this.completionWindow.CompletionList.CompletionData;
                foreach (var item in results)
                {
                    data.Add(item);
                }

                this.completionWindow.Show();
                this.completionWindow.Closed += delegate
                {
                    this.completionWindow = null;
                };
            }
        }

        private sealed class LinqPadCompletionWindow : CompletionWindow
        {
            static LinqPadCompletionWindow()
            {
                WindowStyleProperty.OverrideMetadata(
                    typeof(LinqPadCompletionWindow),
                    new FrameworkPropertyMetadata(WindowStyle.None));

                AllowsTransparencyProperty.OverrideMetadata(
                    typeof(LinqPadCompletionWindow),
                    new FrameworkPropertyMetadata(true));
            }

            public LinqPadCompletionWindow(TextArea textarea)
                : base(textarea)
            {
            }
        }
    }
}
