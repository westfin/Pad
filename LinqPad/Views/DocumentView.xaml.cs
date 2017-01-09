using System;
using System.Linq;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;

using LinqPad.Editor;
using LinqPad.ViewModels;

using Microsoft.CodeAnalysis;

namespace LinqPad.Views
{
    using System.Text;

    /// <summary>
    /// Логика взаимодействия для DocumentView.xaml
    /// </summary>
    public partial class DocumentView 
    {
        private OpenDocumentViewModel viewModel;

        private DiagnosticsService diagnosticService;

        private LnqPadColorizerService colorizerService;

        private ReferencesProvider referencesProvider;

        public DocumentView()
        {
            this.InitializeComponent();
        }

        private static Color GetDiagnosticColor(Diagnostic diagnostic)
        {
            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Info:
                    return Colors.LimeGreen;
                case DiagnosticSeverity.Warning:
                    return Colors.DodgerBlue;
                case DiagnosticSeverity.Hidden:
                    return Colors.Green;
                case DiagnosticSeverity.Error:
                    return Colors.Red;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void DocumentViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.viewModel = (OpenDocumentViewModel)e.NewValue;
            var container = new LinqPadSourceTextContainer(this.editor);
            this.viewModel.Init(container);

            this.editor.IntellisenseProvider = new IntellisenseProvider(
                this.viewModel.MainViewModel.LinqPadEditorHost,
                this.viewModel.DocumentId);

            this.editor.SignatureHelpService = new SignatureHelpService(
                this.viewModel.MainViewModel.LinqPadEditorHost,
                this.viewModel.DocumentId);

            var text = await this.viewModel.LoadText();

            this.referencesProvider = new ReferencesProvider(this.viewModel.MainViewModel.LinqPadEditorHost);
            this.diagnosticService  = new DiagnosticsService(this.viewModel.MainViewModel.LinqPadEditorHost);
            this.colorizerService   = new LnqPadColorizerService(this.editor);
            this.editor.TextArea.TextView.BackgroundRenderers.Add(this.colorizerService);

            this.editor.AppendText(text);
            this.editor.TextChanged += this.EditorTextChanged;
            this.editor.ToolTipRequest = this.ToolTipRequest;
        }

        private async void EditorTextChanged(object sender, EventArgs e)
        {
            await this.ProcessDiagnostics();
        }

        private async Task ProcessDiagnostics()
        {
            this.colorizerService.Clear();
            var diagnostics = await this.diagnosticService.GetDiagnostics(this.viewModel.DocumentId);
            foreach (var diagnostic in diagnostics)
            {
                var start  = diagnostic.Location.SourceSpan.Start;
                var length = diagnostic.Location.SourceSpan.End - diagnostic.Location.SourceSpan.Start;
                var marker = this.colorizerService.TryAdd(start, length);
                if (marker == null)
                {
                    continue;
                }

                marker.MarkerColor = GetDiagnosticColor(diagnostic);
                marker.ToolTip = diagnostic.GetMessage();
            }
        }


        private void ToolTipRequest(ToolTipArgs args)
        {
            if (!args.InDocument)
            {
                return;
            }

            var offset = this.editor.Document.GetOffset(args.LogicalPosition);

            var markersAtOffset = this.colorizerService.GetMarkersAtOffset(offset);
            var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);
            if (markerWithToolTip != null)
            {
                args.SetToolTip(markerWithToolTip.ToolTip);
            }
        }
    }
}
