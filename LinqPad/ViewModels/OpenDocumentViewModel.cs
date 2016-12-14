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
        public event PropertyChangedEventHandler PropertyChanged;

        public OpenDocumentViewModel(MainViewModel mainViewModel, DocumentViewModel document)
        {
            this.mainViewModel = mainViewModel;
            this.document = document;
            this.RunCommand = new DelegateCommand(Run, ()=> !isRunning);
            this.scriptRunner = new ScriptRunner(
                references: mainViewModel.RoslynHost.DefaultReferences.
                    OfType<PortableExecutableReference>().
                    Select(i=> i.FilePath),
                imports:    mainViewModel.RoslynHost.DefaultImports);

            LinqPadExtensions.Dumped += LinqPadExtensions_Dumped;
            results = new ObservableCollection<ResultObject>();
        }

        private void LinqPadExtensions_Dumped(object arg1, string arg2)
        {
            results.Add(new ResultObject(value: arg1, header: arg2));
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
            var code = await GetTextCode().ConfigureAwait(true);
            await scriptRunner.ExecuteAsync(code).ConfigureAwait(true);
            IsRunning = false;
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
    }
}
