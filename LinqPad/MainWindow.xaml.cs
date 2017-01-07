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

using LinqPad.Editor;
using LinqPad.ViewModels;

using Microsoft.CodeAnalysis.CSharp.Scripting;

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
            this.InitializeComponent();
            this.DataContext = this.mainViewModel;
            Console.SetOut(new LinqPadOut());
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var documentViewModel = ((FrameworkElement)e.Source).DataContext as DocumentViewModel;
                if (documentViewModel != null)
                {
                    this.mainViewModel.OpenDocument(documentViewModel);
                }
            }
        }
    }
}
