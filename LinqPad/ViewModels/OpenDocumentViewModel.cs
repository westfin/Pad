using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using LinqPad.Commands;
using LinqPad.Editor;
using LinqPad.Execution;

using LinqPadHosting;

using Microsoft.CodeAnalysis;

namespace LinqPad.ViewModels
{
    using System.Windows;

    using Microsoft.CodeAnalysis.Scripting;

    public class OpenDocumentViewModel : INotifyPropertyChanged
    {
        private readonly ScriptRunner scriptRunner;

        private readonly LinqPadHost linqPadHost;

        private readonly Dispatcher dispatcher;

        private bool isRunning;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public OpenDocumentViewModel(MainViewModel mainViewModel, DocumentViewModel document)
        {
            this.dispatcher    = Dispatcher.CurrentDispatcher;
            this.MainViewModel = mainViewModel;
            this.Document      = document;

            this.RunCommand     = new DelegateCommand(this.Run, () => !this.isRunning);
            this.StopCommand    = new DelegateCommand(new Action(this.Stop));
            this.RestartCommand = new DelegateCommand(this.Restart, () => !this.isRunning);

            this.scriptRunner = new ScriptRunner(
                refs: mainViewModel.LinqPadEditorHost.DefaultReferences.OfType<PortableExecutableReference>().Select(i => i.FilePath),
                imps: mainViewModel.LinqPadEditorHost.DefaultImports);

            this.linqPadHost = new LinqPadHost(
                    references: mainViewModel.LinqPadEditorHost.DefaultReferences.OfType<PortableExecutableReference>().Select(i => i.FilePath),
                    imports: mainViewModel.LinqPadEditorHost.DefaultImports);

            this.scriptRunner.Dumped += this.ScriptRunnerDumped;
            this.Results = new ObservableCollection<ResultObject>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => this.Document != null ? this.Document.Name : "New";

        public DocumentId DocumentId { get; private set; }

        public MainViewModel MainViewModel { get; }

        public ObservableCollection<ResultObject> Results { get; }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }

            private set
            {
                if (this.isRunning == value)
                {
                    return;
                }

                this.isRunning = value;
                this.OnRaisePropertyChanged(nameof(this.IsRunning));
            }
        }

        // Delegates commands
        public DelegateCommand RunCommand { get; }

        public DelegateCommand StopCommand { get; }

        public DelegateCommand RestartCommand { get; }

        private DocumentViewModel Document { get; }

        public void Init(LinqPadSourceTextContainer container)
        {
            this.DocumentId = this.MainViewModel.LinqPadEditorHost.AddDocument(container);
        }

        public Document GetDocument()
        {
            return this.MainViewModel.LinqPadEditorHost.GetDocument(this.DocumentId);
        }

        public async Task<string> LoadText()
        {
            if (this.Document == null)
            {
                return string.Empty;
            }

            using (var stream = new StreamReader(new FileStream(this.Document.Path, FileMode.Open)))
            {
                return await stream.ReadToEndAsync();
            }
        }

        private void ScriptRunnerDumped(IList<ResultObject> obj)
        {
            this.dispatcher.InvokeAsync(() =>
            {
                foreach (var item in obj)
                {
                    this.Results.Add(item);
                }
            });
        }

        private async Task Run()
        {
            this.IsRunning = true;
            this.Results.Clear();
            this.MainViewModel.ChartViewModel.ClearCharts();
            this.MainViewModel.DataGridViewModel.ClearTables();

            var code = await this.GetTextCode().ConfigureAwait(true);

            try
            {
                await this.scriptRunner.ExecuteAsync(code: code, token: this.cancellationTokenSource.Token)
                    .ConfigureAwait(true);
            }
            catch (CompilationErrorException e)
            {
                foreach (var diagnostic in e.Diagnostics)
                {
                    this.Results.Add(new ResultObject(
                        value: diagnostic,
                        depth: 0,
                        header: "exception"));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private async Task Restart()
        {
            this.Stop();
            await this.Run();
        }

        private void Stop()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
            }

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task<string> GetTextCode()
        {
            var text = await this.MainViewModel
                .LinqPadEditorHost
                .GetDocument(this.DocumentId)
                .GetTextAsync()
                .ConfigureAwait(false);
            return text.ToString();
        }

        private void OnRaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
