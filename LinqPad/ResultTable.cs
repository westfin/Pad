﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad
{
    public sealed class ResultTable<T>
    {
        public ResultTable()
        {
            this.ItemsSource = new ObservableCollection<T>();
        }

        public ObservableCollection<T> ItemsSource { get; set; }

        public string Title { get; set; }
    }
}
