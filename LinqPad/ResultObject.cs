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
        private string header;
        public  string Header => header;
        private object value;
        public  object Value => value;
        private string typeName;
        public string  TypeName => typeName;


        private ObservableCollection<ResultObject> children;
        public  ObservableCollection<ResultObject> Children => children;

        public ResultObject(object value, string header = "")
        {
            this.header   = header;
            this.value    = value;
            this.typeName = value.GetType().Name;
            var enumerable = value as IEnumerable;
            children = new ObservableCollection<ResultObject>();
            if (enumerable !=null)
            {
                foreach (var item in enumerable)
                {
                    children.Add(new ResultObject(item));
                }
            }
        }
    }
}
