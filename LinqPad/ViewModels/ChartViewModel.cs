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

        public ChartViewModel()
        {
            this.PlotModels = new ObservableCollection<PlotModel>();

            this.NextCommand    = new DelegateCommand(new Action(this.Next));
            this.PreviosCommand = new DelegateCommand(new Action(this.Previos));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PlotModel CurrentPlotModel
        {
            get
            {
                return this.currentPlotModel;
            }

            set
            {
                if (this.currentPlotModel == value)
                {
                    return;
                }
                this.currentPlotModel = value;
                this.OnRaisePropertyChanged(nameof(this.CurrentPlotModel));
            }
        }

        public DelegateCommand NextCommand { get; }

        public DelegateCommand PreviosCommand { get; }

        public ObservableCollection<PlotModel> PlotModels { get; }

        public void AddChart(PlotModel chart)
        {
            if (chart == null)
            {
                throw new ArgumentNullException(nameof(chart));
            }
            this.PlotModels.Add(chart);
            this.CurrentPlotModel = chart;
        }

        public void ClearCharts()
        {
            this.PlotModels.Clear();
            this.CurrentPlotModel = null;
        }


        private void Next()
        {
            if (this.currentPlotModel == null)
            {
                return;
            }
            var nextid = this.PlotModels.IndexOf(this.currentPlotModel) + 1;
            if (nextid < this.PlotModels.Count)
            {
                this.CurrentPlotModel = this.PlotModels.ElementAt(nextid);
            }
        }

        private void Previos()
        {
            if (this.currentPlotModel == null)
            {
                return;
            }

            var previd = this.PlotModels.IndexOf(this.currentPlotModel) - 1;
            if (previd >= 0)
            {
                this.CurrentPlotModel = this.PlotModels.ElementAt(previd);
            }
        }

        private void OnRaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
