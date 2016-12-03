using LinqPad.Editor;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.ViewModels
{
    public class OpenDocumentViewModel
    {
        private DocumentViewModel document;
        private MainViewModel mainViewModel;
        private DocumentId documentId;

        public OpenDocumentViewModel(MainViewModel mainViewModel, DocumentViewModel document)
        {
            this.mainViewModel = mainViewModel;
            this.document = document;
        }


        public DocumentViewModel Document       => document;
        public DocumentId        DocumentId     => documentId;
        public MainViewModel     MainViewModel  => mainViewModel;


        public void Init(LinqPadSourceTextContainer container)
        {
            documentId = mainViewModel.RoslynHost.AddDocument(container);
        }

        public string Title { get { return Document != null ? Document.Name : "New"; } }

    }
}
