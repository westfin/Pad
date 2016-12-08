using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using Microsoft.CodeAnalysis;
using System.Windows.Documents;
using System.Windows.Media;

namespace LinqPad.Editor
{
    class OverloadProvider : IOverloadProvider
    {
        private readonly SignatureHelp signature;

        private int    selectedIndex;
        private object currentHeader;
        private object currentContent;
        private string currentIndexText;

        public OverloadProvider(SignatureHelp signature)
        {
            this.signature = signature;
            if(signature.ActiveSignature > 0)
            {
                selectedIndex = signature.ActiveSignature;
            }
            BuildContent();
        }

        public int Count => signature.SignatureHelpItems.Count();

        public object CurrentContent
        {
            get { return currentContent; }
            set
            {
                if (currentContent == value)
                    return;
                currentContent = value;
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        public object CurrentHeader
        {
            get { return currentHeader; }
            private set
            {
                if (currentHeader == value)
                    return;
                currentHeader = value;
                OnPropertyChanged(nameof(CurrentHeader));
            }
        }

        public string CurrentIndexText
        {
            get { return currentIndexText; }
            set
            {
                if (currentIndexText == null)
                    return;
                currentIndexText = value;
                OnPropertyChanged(nameof(CurrentIndexText));
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }

            set
            {
                if (selectedIndex == value)
                    return;
                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
                BuildContent();
            }
        }

        private void BuildContent()
        {
            var item = signature.SignatureHelpItems[selectedIndex];
            var counter = new TextBlock()
            {
                Text = $"({selectedIndex + 1}:{signature.SignatureHelpItems.Count}) ",
                FontWeight = FontWeights.Bold
            };

            var headerPanel = new WrapPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            counter,
                            item.Lable.ToTextBlock()
                        }
                    }
                }
            };
            var contetnPanel = new StackPanel()
            {
                Children = { new TextBlock() { Text = item.Documentation } }
            };

            if(selectedIndex <= item.Parametrs.Count() && selectedIndex > 0)
            {
                var param = item.Parametrs.ToArray()[signature.ActiveParameter];
                if (signature.ActiveParameter < item.Parametrs.Count())
                    contetnPanel.Children.Add(param.Lable.ToTextBlock());
            }

            CurrentHeader  = headerPanel;
            CurrentContent = contetnPanel;
        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public static class SymbolDisplayPartExtension
    {
        public static TextBlock ToTextBlock(this IEnumerable<SymbolDisplayPart> parts)
        {
            var textBlock = new TextBlock();
            foreach (var item in parts)
            {
                textBlock.Inlines.Add(item.ToRun());
            }

            return textBlock;
        }

        public static Run ToRun(this SymbolDisplayPart text)
        {
            var run = new Run(text.ToString());
            switch (text.Kind)
            {
                case SymbolDisplayPartKind.Keyword:
                    run.Foreground = Brushes.Blue;
                    break;
                case SymbolDisplayPartKind.ClassName:
                case SymbolDisplayPartKind.DelegateName:
                case SymbolDisplayPartKind.EnumName:
                case SymbolDisplayPartKind.EventName:
                case SymbolDisplayPartKind.InterfaceName:
                case SymbolDisplayPartKind.StructName:
                    run.Foreground = Brushes.Teal;
                    break;
            }

            return run;
        }
    }
}
