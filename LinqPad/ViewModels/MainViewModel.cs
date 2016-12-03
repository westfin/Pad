using LinqPad.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private OpenDocumentViewModel currentDocumentViewModel;
        private ObservableCollection<OpenDocumentViewModel> openDocuments;

        private readonly RoslynEditorHost roslynHost = new RoslynEditorHost();
        public RoslynEditorHost RoslynHost
        {
            get { return roslynHost; }
        }

        public MainViewModel()
        {
            OpenDocuments = new ObservableCollection<OpenDocumentViewModel>();

            if (!IsOpenAnyDocuments)
                CreateDocument();
        }

        public ObservableCollection<OpenDocumentViewModel> OpenDocuments
        {
            get { return openDocuments; }
            set
            {
                if (value == openDocuments)
                    return;
                openDocuments = value;
                OnPropertyChanged(nameof(OpenDocuments));
            }
        }

        public string Title
        {
            get { return "LinqPad"; }
        }

        public bool IsOpenAnyDocuments => openDocuments.Count != 0;

        public OpenDocumentViewModel CurrentDocumentViewModel
        {
            get { return currentDocumentViewModel; }
            set
            {
                if (value == currentDocumentViewModel)
                    return;
                currentDocumentViewModel = value;
                OnPropertyChanged(nameof(CurrentDocumentViewModel));
            }
        }

        private void CreateDocument()
        {
            var document = new OpenDocumentViewModel(this, null);
            currentDocumentViewModel = document;
            openDocuments.Add(document);
        }

        private void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
