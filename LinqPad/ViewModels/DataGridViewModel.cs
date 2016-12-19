using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqPad.Commands;

namespace LinqPad.ViewModels
{
    public sealed class DataGridViewModel : INotifyPropertyChanged
    {
        private ResultTable<object> currentTable;
        public  ResultTable<object> CurrentTable
        {
            get { return currentTable; }
            set
            {
                if (currentTable == value)
                    return;
                currentTable = value;
                OnRaisePropertyChanged(nameof(CurrentTable));
            }
        }

        public ObservableCollection<ResultTable<object>> Tables { get; }
        public DelegateCommand NextCommand    { get; }
        public DelegateCommand PreviosCommand { get; }

        public DataGridViewModel()
        {
            Tables = new ObservableCollection<ResultTable<object>>();
            AddTable(new ResultTable<object>() { Title = "test title" });

            NextCommand    = new DelegateCommand(new Action(Next));
            PreviosCommand = new DelegateCommand(new Action(Previos));
        }

        private void Next()
        {
            if(currentTable == null)
            {
                return;
            }
            var nextid = Tables.IndexOf(currentTable) + 1;
            if (nextid < Tables.Count)
            {
                CurrentTable = Tables.ElementAt(nextid);
            }
        }

        private void Previos()
        {
            if (currentTable == null)
            {
                return;
            }
            var previd = Tables.IndexOf(currentTable) - 1;
            if (previd >= 0)
            {
                CurrentTable = Tables.ElementAt(previd);
            }
        }

        public void AddTable(ResultTable<object> table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            Tables.Add(table);
            CurrentTable = table;
        }

        public void ClearTables()
        {
            Tables.Clear();
            CurrentTable = null;
        }

        private void OnRaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
