using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LinqPad.Commands;

namespace LinqPad.ViewModels
{
    public sealed class DataGridViewModel : INotifyPropertyChanged
    {
        private ResultTable<object> currentTable;

        public DataGridViewModel()
        {
            this.Tables = new ObservableCollection<ResultTable<object>>();
            this.AddTable(new ResultTable<object>() { Title = "test title" });

            this.NextCommand    = new DelegateCommand(new Action(this.Next));
            this.PreviosCommand = new DelegateCommand(new Action(this.Previos));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ResultTable<object>> Tables { get; }

        public DelegateCommand NextCommand { get; }

        public DelegateCommand PreviosCommand { get; }

        public ResultTable<object> CurrentTable
        {
            get
            {
                return this.currentTable;
            }

            set
            {
                if (this.currentTable == value)
                {
                    return;
                }

                this.currentTable = value;
                this.OnRaisePropertyChanged(nameof(this.CurrentTable));
            }
        }

        public void AddTable(ResultTable<object> table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            this.Tables.Add(table);
            this.CurrentTable = table;
        }

        public void ClearTables()
        {
            this.Tables.Clear();
            this.CurrentTable = null;
        }

        private void Next()
        {
            if (this.currentTable == null)
            {
                return;
            }

            var nextid = this.Tables.IndexOf(this.currentTable) + 1;
            if (nextid < this.Tables.Count)
            {
                this.CurrentTable = this.Tables.ElementAt(nextid);
            }
        }

        private void Previos()
        {
            if (this.currentTable == null)
            {
                return;
            }

            var previd = this.Tables.IndexOf(this.currentTable) - 1;
            if (previd >= 0)
            {
                this.CurrentTable = this.Tables.ElementAt(previd);
            }
        }

        private void OnRaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
