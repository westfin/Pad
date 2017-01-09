using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.CodeCompletion;

using Microsoft.CodeAnalysis;

namespace LinqPad.Editor
{
    public sealed class OverloadProvider : IOverloadProvider
    {
        private readonly SignatureHelp signature;

        private int selectedIndex;
        private object currentHeader;
        private object currentContent;
        private string currentIndexText;

        public OverloadProvider(SignatureHelp signature)
        {
            this.signature = signature;
            if (signature.ActiveSignature > 0)
            {
                this.selectedIndex = signature.ActiveSignature;
            }

            this.BuildContent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Count => this.signature.SignatureHelpItems.Count();

        public object CurrentContent
        {
            get
            {
                return this.currentContent;
            }

            private set
            {
                if (this.currentContent == value)
                {
                    return;
                }

                this.currentContent = value;
                this.OnPropertyChanged(nameof(this.CurrentContent));
            }
        }

        public object CurrentHeader
        {
            get
            {
                return this.currentHeader;
            }

            private set
            {
                if (this.currentHeader == value)
                {
                    return;
                }

                this.currentHeader = value;
                this.OnPropertyChanged(nameof(this.CurrentHeader));
            }
        }

        public string CurrentIndexText
        {
            get
            {
                return this.currentIndexText;
            }

            set
            {
                if (this.currentIndexText == null)
                {
                    return;
                }

                this.currentIndexText = value;
                this.OnPropertyChanged(nameof(this.CurrentIndexText));
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this.selectedIndex;
            }

            set
            {
                if (this.selectedIndex == value)
                {
                    return;
                }

                this.selectedIndex = value;
                this.OnPropertyChanged(nameof(this.SelectedIndex));
                this.BuildContent();
            }
        }

        private void BuildContent()
        {
            var item = this.signature.SignatureHelpItems[this.selectedIndex];
            var counter = new TextBlock();
            if (this.signature.SignatureHelpItems.Count > 1)
            {
                counter.Text = $"{this.selectedIndex + 1} of {this.signature.SignatureHelpItems.Count} ";
                counter.FontWeight = FontWeights.Bold;
            }

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

            if (this.signature.ActiveParameter < item.Parametrs.Count() && this.selectedIndex > 0)
            {
                var param = item.Parametrs.ToArray()[this.signature.ActiveParameter];
                if (this.signature.ActiveParameter < item.Parametrs.Count())
                {
                    contetnPanel.Children.Add(param.Lable.ToTextBlock());
                }
            }

            this.CurrentHeader  = headerPanel;
            this.CurrentContent = contetnPanel;
        }

        private void OnPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
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

        private static Run ToRun(this SymbolDisplayPart text)
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
