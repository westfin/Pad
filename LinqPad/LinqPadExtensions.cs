using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;

namespace LinqPad
{
    public static class LinqPadExtensions
    {
        public static event Action<object, string> Dumped;

        public static event Action<PlotModel> Ploted;

        public static event Action<ResultTable<object>> Tabled;

        public static T Dump<T>(this T obj, string nameval = "")
        {
            Dumped?.Invoke(obj, nameval);
            return obj;
        }
        
        public static PlotModel Plot(this PlotModel plot)
        {
            Ploted?.Invoke(plot);
            return plot;
        }

        public static IEnumerable<T> Table<T>(this IEnumerable<T> enumerable, string title)
        {
            Tabled?.Invoke(new ResultTable<object>()
            {
                Title = title,
                ItemsSource = new ObservableCollection<object>(enumerable.Cast<object>())
            });
            return enumerable;
        }

        public static bool IsPrimitiveType(this Type type)
        {
            return type.IsPrimitive 
                || type == typeof(string) 
                || type == typeof(decimal);
        }
    }
}
