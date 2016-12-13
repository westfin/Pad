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

namespace LinqPad.ViewModels
{
    public class OpenDocumentViewModel
    {
        private DocumentViewModel     document;
        private MainViewModel         mainViewModel;
        private DocumentId            documentId;
        private readonly ScriptRunner scriptRunner;
        public  ObservableCollection<ResultObject> results;

        public OpenDocumentViewModel(MainViewModel mainViewModel, DocumentViewModel document)
        {
            this.mainViewModel = mainViewModel;
            this.document = document;
            this.RunCommand = new DelegateCommand(new Action(Run));
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

        //Delegates commands
        public DelegateCommand   RunCommand     { get; }


        public void Init(LinqPadSourceTextContainer container)
        {
            documentId = mainViewModel.RoslynHost.AddDocument(container);
        }

        public Document GetDocument()
        {
            return MainViewModel.RoslynHost.GetDocument(documentId);
        }

        private async void Run()
        {
            results.Clear();
            var code = await GetTextCode().ConfigureAwait(true);
            await scriptRunner.ExecuteAsync(code);
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
    }
}
