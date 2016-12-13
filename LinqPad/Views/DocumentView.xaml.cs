using System.Linq;
using LinqPad.Editor;
using LinqPad.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace LinqPad.Views
{
    /// <summary>
    /// Логика взаимодействия для DocumentView.xaml
    /// </summary>
    public partial class DocumentView 
    {
        private OpenDocumentViewModel  viewModel;
        private DiagnosticsService     diagnosticService;
        private LnqPadColorizerService colorizerService;
        private ReferencesProvider     referencesProvider;

        public DocumentView()
        {
            InitializeComponent();
        }

        private async void DocumentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = (OpenDocumentViewModel)e.NewValue;
            LinqPadSourceTextContainer container = new LinqPadSourceTextContainer(editor);
            viewModel.Init(container);
            editor.IntellisenseProvider = new IntellisenseProvider(
                editor,
                viewModel.MainViewModel.RoslynHost,
                viewModel.DocumentId);

            editor.SignatureHelpService = new SignatureHelpService(
                viewModel.MainViewModel.RoslynHost,
                viewModel.DocumentId);

            string text = await viewModel.LoadText();

            referencesProvider = new ReferencesProvider(viewModel.MainViewModel.RoslynHost);
            colorizerService = new LnqPadColorizerService(editor);
            editor.TextArea.TextView.BackgroundRenderers.Add(colorizerService);
            diagnosticService = new DiagnosticsService(viewModel.MainViewModel.RoslynHost);
            editor.AppendText(text);
            editor.TextChanged += Editor_TextChanged;
            editor.ToolTipRequest = ToolTipRequest;
        }

        private async void Editor_TextChanged(object sender, EventArgs e)
        {
            await ProcessDiagnostics();
        }

        private async Task ProcessReferences()
        {
            var references = await referencesProvider.
                GetReferences(viewModel.DocumentId);

            var host = viewModel.MainViewModel.RoslynHost;

            if (references == null)
                return;

            if(File.Exists(references.First()))
            {
                host.ProcessResolveReferences(viewModel.DocumentId, references);
            }
        }

        private async Task ProcessDiagnostics()
        {
            colorizerService.Clear();
            var diagnostics = await diagnosticService.GetDiagnostics(viewModel.DocumentId);
            foreach (var diagnostic in diagnostics)
            {
                var start = diagnostic.Location.SourceSpan.Start;
                var length = diagnostic.Location.SourceSpan.End - diagnostic.Location.SourceSpan.Start;
                var marker = colorizerService.TryAdd(start, length);
                if (marker != null)
                {
                    marker.MarkerColor = GetDiagnosticColor(diagnostic);
                    marker.ToolTip = diagnostic.GetMessage();
                }
            }
        }


        private void ToolTipRequest(ToolTipArgs args)
        {
            if (!args.InDocument)
                return;

            var offset = editor.Document.GetOffset(args.LogicalPosition);

            var markersAtOffset = colorizerService.GetMarkersAtOffset(offset);
            var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);
            if (markerWithToolTip != null)
            {
                args.SetToolTip(markerWithToolTip.ToolTip);
            }
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
    }
}
