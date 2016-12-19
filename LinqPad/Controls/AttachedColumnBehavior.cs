using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LinqPad.Controls
{
    public class AttachedColumnBehavior
    {
        public static readonly DependencyProperty AttachedColumnsProperty =
            DependencyProperty.RegisterAttached(
            "AttachedColumns",
            typeof(IEnumerable),
            typeof(AttachedColumnBehavior),
            new UIPropertyMetadata(null, OnAttachedColumnPropertyChanged)
        );

        public static IEnumerable GetAttachedColumns(DependencyObject dataGrid)
        {
            return (IEnumerable)dataGrid.GetValue(AttachedColumnsProperty);
        }

        public static void SetAttachedColumns(DependencyObject dataGrid, IEnumerable value)
        {
            dataGrid.SetValue(AttachedColumnsProperty, value);
        }

        private static void OnAttachedColumnPropertyChanged(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            var columns = e.NewValue as INotifyCollectionChanged;
            if (columns != null)
            {
                columns.CollectionChanged += (sender, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Remove)
                    {
                        RemoveColumn(dataGrid, args.OldItems);
                    }
                    else if(args.Action == NotifyCollectionChangedAction.Add)
                    {
                        AddColumn(dataGrid, args.NewItems);
                    }
                };
            }
        }

        private static void RemoveColumn(DataGrid dataGrid, IEnumerable columns)
        {
            foreach (var column in columns)
            {
                var col = dataGrid.Columns.Where(i => i.Header == column).Single();
                dataGrid.Columns.Remove(col);
            }
        }

        private static void AddColumn(DataGrid dataGrid, IEnumerable columns)
        {
            foreach (var column in columns)
            {
                var col = dataGrid.Columns.Where(i => i.Header == column).Single();
                dataGrid.Columns.Add(col);
            }
        }
    }
}
