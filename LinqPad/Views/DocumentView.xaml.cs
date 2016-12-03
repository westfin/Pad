using LinqPad.Editor;
using LinqPad.ViewModels;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis;

namespace LinqPad.Views
{
    /// <summary>
    /// Логика взаимодействия для DocumentView.xaml
    /// </summary>
    public partial class DocumentView 
    {
        private OpenDocumentViewModel viewModel;

        public DocumentView()
        {
            InitializeComponent();
        }

        private void DocumentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = (OpenDocumentViewModel)e.NewValue;
            LinqPadSourceTextContainer container = new LinqPadSourceTextContainer(editor);
            viewModel.Init(container);
            editor.IntellisenseProvider = new IntellisenseProvider(
                editor,
                viewModel.MainViewModel.RoslynHost,
                viewModel.DocumentId);
        }
    }
}
