using System;
using LinqPad.Editor;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LinqPad.Commands;
using LinqPad.Execution;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using LinqPadHosting;

namespace LinqPad.ViewModels
{
    public class OpenDocumentViewModel : INotifyPropertyChanged
    {
        private readonly DocumentViewModel document;

        private readonly MainViewModel mainViewModel;

        private readonly ScriptRunner scriptRunner;

        private readonly LinqPadHost linqPadHost;

        private readonly ObservableCollection<ResultObject> results;

        private readonly Dispatcher dispatcher;

        private DocumentId documentId;

        private bool isRunning;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public OpenDocumentViewModel(MainViewModel mainViewModel, DocumentViewModel document)
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.mainViewModel = mainViewModel;
            this.document = document;

            this.RunCommand     = new DelegateCommand(this.Run, () => !this.isRunning);
            this.StopCommand    = new DelegateCommand(new Action(this.Stop));
            this.RestartCommand = new DelegateCommand(this.Restart, () => !this.isRunning);
            this.scriptRunner   = new ScriptRunner(
                references: mainViewModel.RoslynHost.DefaultReferences.OfType<PortableExecutableReference>().Select(i => i.FilePath),
                imports: mainViewModel.RoslynHost.DefaultImports);

            this.linqPadHost =
                new LinqPadHost(
                    references: mainViewModel.RoslynHost.DefaultReferences.OfType<PortableExecutableReference>().Select(i => i.FilePath),
                    imports: mainViewModel.RoslynHost.DefaultImports);

            this.scriptRunner.Dumped += this.ScriptRunnerDumped;
            this.results = new ObservableCollection<ResultObject>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => this.Document != null ? this.Document.Name : "New";

        public DocumentViewModel Document => this.document;

        public DocumentId DocumentId => this.documentId;

        public MainViewModel MainViewModel => this.mainViewModel;

        public ObservableCollection<ResultObject> Results => this.results;

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

        public void Init(LinqPadSourceTextContainer container)
        {
            this.documentId = this.mainViewModel.RoslynHost.AddDocument(container);
        }

        public Document GetDocument()
        {
            return this.MainViewModel.RoslynHost.GetDocument(this.documentId);
        }

        public async Task<string> LoadText()
        {
            if (this.document == null)
            {
                return string.Empty;
            }

            using (var stream = new StreamReader(new FileStream(this.document.Path, FileMode.Open)))
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
                    this.results.Add(item);
                }
            });
        }

        private async Task Run()
        {
            this.IsRunning = true;
            this.results.Clear();
            this.mainViewModel.ChartViewModel.ClearCharts();
            this.mainViewModel.DataGridViewModel.ClearTables();
            var code = await this.GetTextCode().ConfigureAwait(true);
            await this.scriptRunner.ExecuteAsync(code, this.cancellationTokenSource.Token).ConfigureAwait(true);

            // await linqPadHost.ExecuteAsync(code, cancellationTokenSource.Token);
            this.IsRunning = false;
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
            var text =
                await this.mainViewModel.RoslynHost.GetDocument(this.documentId).GetTextAsync().ConfigureAwait(false);

            return text.ToString();
        }


        private void OnRaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
