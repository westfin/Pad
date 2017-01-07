using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqProviders.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ExcelColumnAttribute : Attribute
    {
        private readonly string columnName;
        public ExcelColumnAttribute(string columnName)
        {
            this.columnName = columnName;
        }

        public string ColumnName { get { return columnName; } }
    }
}
