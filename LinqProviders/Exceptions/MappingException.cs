using System;
using System.Collections.Generic;
using System.Text;

namespace LinqProviders.Exceptions
{
    class MappingException : Exception
    {
        private readonly string mappedProp;
        private readonly List<string> columnNames;
        private readonly ExcelQueryArgs args;

        public MappingException(string prop, ExcelQueryArgs args)
        {
            mappedProp = prop;
            this.args  = args;
            columnNames = (List<string>)ExcelHelperClass.GetColumnNames(args);
        }

        public override string Message
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in columnNames)
                {
                    sb.Append(item + " ");
                }
                return String.Format($"is not valid column name '{mappedProp}', valid column names: '{sb}' in file '{args.FileName}', in Sheet '{args.SheetName}'");
            }
        }
    }
}
