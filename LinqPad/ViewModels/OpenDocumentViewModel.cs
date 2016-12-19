using LinqPad.Editor;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqPad.Commands;
using System.Windows;
using LinqPad.Execution;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Rendering;
using System.Threading;
using LinqPadHosting;

namespace LinqPad.ViewModels
{
    public class OpenDocumentViewModel : INotifyPropertyChanged
    {
        private DocumentViewModel     document;
        private MainViewModel         mainViewModel;
        private DocumentId            documentId;
        private readonly ScriptRunner scriptRunner;
        private bool isRunning = false;
        public  ObservableCollection<ResultObject> results;
        private Dispatcher dispatcher;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public OpenDocumentViewModel(MainViewModel mainViewModel, DocumentViewModel document)
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.mainViewModel = mainViewModel;
            this.document = document;

            this.RunCommand     = new DelegateCommand(Run, ()=> !isRunning);
            this.StopCommand    = new DelegateCommand(new Action(Stop));
            this.RestartCommand = new DelegateCommand(Restart, () => !isRunning);
            this.scriptRunner = new ScriptRunner(
                references: mainViewModel.RoslynHost.DefaultReferences.
                    OfType<PortableExecutableReference>().Select(i=> i.FilePath),
                imports: mainViewModel.RoslynHost.DefaultImports);

            scriptRunner.Dumped += ScriptRunner_Dumped; ;
            results = new ObservableCollection<ResultObject>();
        }

        private void ScriptRunner_Dumped(IList<ResultObject> obj)
        {
            dispatcher.InvokeAsync(()=>
            {
                foreach (var item in obj)
                {
                    results.Add(item);
                }
            });
        }

        public DocumentViewModel Document           => document;
        public DocumentId        DocumentId         => documentId;
        public MainViewModel     MainViewModel      => mainViewModel;
        public ObservableCollection<ResultObject> Results => results;
        public bool IsRunning
        {
            get { return isRunning; }
            private set
            {
                if (isRunning == value)
                    return;
                isRunning = value;
                OnRaisePropertyChanged(nameof(IsRunning));
            }
        }


        //Delegates commands
        public DelegateCommand RunCommand     { get; }
        public DelegateCommand StopCommand    { get; }
        public DelegateCommand RestartCommand { get; }


        public void Init(LinqPadSourceTextContainer container)
        {
            documentId = mainViewModel.RoslynHost.AddDocument(container);
        }

        public Document GetDocument()
        {
            return MainViewModel.RoslynHost.GetDocument(documentId);
        }

        private async Task Run()
        {
            IsRunning = true;
            results.Clear();
            mainViewModel.ChartViewModel.ClearCharts();
            mainViewModel.DataGridViewModel.ClearTables();
            var code = await GetTextCode().ConfigureAwait(true);
            //await scriptRunner.ExecuteAsync(code, cancellationTokenSource.Token).ConfigureAwait(true);
            LinqPadHost host = new LinqPadHost();
            await host.ExecuteAsync(code, cancellationTokenSource.Token);
            IsRunning = false;
        }

        private async Task Restart()
        {
            Stop();
            await Run();
        }

        private void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task<string> GetTextCode()
        {
            var text = await mainViewModel.
                RoslynHost.GetDocument(documentId).
                GetTextAsync().ConfigureAwait(false);

            return text.ToString();
        }

        public async Task<string> LoadText()
        {
            if (document == null)
            {
                return "";
            }

            using (var stream = new StreamReader(new FileStream(document.Path, FileMode.Open)))
            {
                return await stream.ReadToEndAsync();
            }
        }

        public string Title
        {
            get { return Document != null ? Document.Name : "New"; }
        }

        private void OnRaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
