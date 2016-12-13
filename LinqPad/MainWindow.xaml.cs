using LinqPad.ViewModels;
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
using Microsoft.CodeAnalysis.CSharp.Scripting;
using LinqPad.Editor;

namespace LinqPad
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mainViewModel =
            new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainViewModel;
            Console.SetOut(new LinqPadOut());
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var documentViewModel = ((FrameworkElement)e.Source).DataContext as DocumentViewModel;
            if (documentViewModel != null)
            {
                mainViewModel.OpenDocument(documentViewModel);
            }
        }
    }
}
