using LinqPad.Editor;
using OxyPlot;
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
        private ObservableCollection<DocumentViewModel>     documents;
        
        private readonly RoslynEditorHost roslynHost = new RoslynEditorHost();
        public RoslynEditorHost RoslynHost
        {
            get { return roslynHost; }
        }

        public ChartViewModel    ChartViewModel    { get; }
        public DataGridViewModel DataGridViewModel { get; }

        public MainViewModel()
        {
            OpenDocuments = new ObservableCollection<OpenDocumentViewModel>();
            documents     = new ObservableCollection<DocumentViewModel>();
            documents.Add(new DocumentViewModel(@"C:\Users\Ivan\Documents\RoslynPad\Samples", true));

            ChartViewModel    = new ChartViewModel();
            DataGridViewModel = new DataGridViewModel();

            LinqPadExtensions.Ploted += LinqPadExtensions_Ploted;
            LinqPadExtensions.Tabled += LinqPadExtensions_Tabled;
            if (!IsOpenAnyDocuments)
                CreateDocument();
        }

        private void LinqPadExtensions_Tabled(ResultTable<object> obj)
        {
            if(obj != null)
            {
                DataGridViewModel.AddTable(obj);
            }
        }

        private void LinqPadExtensions_Ploted(PlotModel obj)
        {
            if (obj != null)
            {
                ChartViewModel.AddChart(obj);
            }
        }

        public ObservableCollection<DocumentViewModel> Documents
        {
            get { return documents; }
            set
            {
                if (documents == value)
                    return;
                documents = value;
                OnPropertyChanged(nameof(Documents));
            }
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

        public void OpenDocument(DocumentViewModel source)
        {
            if (!source.IsFolder)
            {
                openDocuments.Add(new OpenDocumentViewModel(this, source));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
