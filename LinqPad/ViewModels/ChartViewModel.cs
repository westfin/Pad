using LinqPad.Commands;
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
    public sealed class ChartViewModel : INotifyPropertyChanged
    {
        private PlotModel currentPlotModel;
        public  PlotModel CurrentPlotModel
        {
            get { return currentPlotModel; }
            set
            {
                if (currentPlotModel == value)
                    return;
                currentPlotModel = value;
                OnRaisePropertyChanged(nameof(CurrentPlotModel));
            }
        }

        private DelegateCommand nextCommand;
        public  DelegateCommand NextCommand => nextCommand;

        private DelegateCommand previosCommand;
        public  DelegateCommand PreviosCommand => previosCommand;

        public  ObservableCollection<PlotModel> PlotModels
        {
            get;
        }

        public ChartViewModel()
        {
            PlotModels = new ObservableCollection<PlotModel>();

            nextCommand    = new DelegateCommand(new Action(Next));
            previosCommand = new DelegateCommand(new Action(Previos));
        }

        private void Next()
        {
            if (currentPlotModel == null)
            {
                return;
            }
            var nextid = PlotModels.IndexOf(currentPlotModel) + 1;
            if (nextid < PlotModels.Count)
            {
                CurrentPlotModel = PlotModels.ElementAt(nextid);
            }
        }

        private void Previos()
        {
            if(currentPlotModel==null)
            {
                return;
            }
            var previd = PlotModels.IndexOf(currentPlotModel) - 1;
            if (previd >= 0)
            {
                CurrentPlotModel = PlotModels.ElementAt(previd);
            }
        }

        public void AddChart(PlotModel chart)
        {
            if(chart == null)
            {
                throw new ArgumentNullException(nameof(chart));
            }
            PlotModels.Add(chart);
            CurrentPlotModel = chart;
        }

        public void ClearCharts()
        {
            PlotModels.Clear();
            CurrentPlotModel = null;
        }

        private void OnRaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
