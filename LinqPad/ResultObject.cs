using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad
{
    using System.Reflection;

    public class ResultObject
    {
        public ResultObject(object value, string header = null)
        {
            this.Childrens = new ObservableCollection<ResultObject>();
            this.BuildObject(value, header);
        }

        public string Header { get; private set; }

        public object Value { get; private set; }

        public string TypeName { get; private set; }

        public ObservableCollection<ResultObject> Childrens { get; }

        private void BuildObject(object value, string header = null)
        {
            if (value == null)
            {
                this.Value = "<null>";
                this.Header = header;
                this.TypeName = string.Empty;
                return;
            }

            var type = value.GetType();
            this.Value = value;
            this.Header = header;
            this.TypeName = type.Name;

            if (type.IsPrimitiveType())
            {
                return;
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                this.Value = $"<enumarable>";
                foreach (var item in enumerable)
                {
                    this.Childrens.Add(new ResultObject(item));
                }

                return;
            }

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var propVal = property?.GetValue(value);
                var propType = propVal?.GetType();
                if (type == propType)
                {
                    continue;    
                }

                this.Childrens.Add(new ResultObject(
                    value: property?.GetValue(value), 
                    header: property?.Name));
            }
        }
    }
}
