using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad
{
    public class ResultObject
    {
        public ResultObject(object value, string header = "")
        {
            this.Header    = header;
            this.Value     = value;
            this.TypeName  = value.GetType().Name;
            this.Childrens = new ObservableCollection<ResultObject>();

            var enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                return;
            }

            foreach (var item in enumerable)
            {
                this.Childrens.Add(new ResultObject(item));
            }

            header = header == string.Empty ? $"enumerable type {this.Childrens.Count}" : header;
        }

        public string Header { get; }

        public object Value { get; }

        public string TypeName { get; }

        public ObservableCollection<ResultObject> Childrens { get; }
    }
}
