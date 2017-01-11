using Remotion.Linq.Parsing.Structure;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqProviders
{
    public class ExcelFactory
    {
        private readonly ExcelQueryArgs args;

        public ExcelFactory(string filename)
        {
            this.args = new ExcelQueryArgs { FileName = filename };
        }

        public ExcelQuery<TSheet> Sheet<TSheet>()
        {
            return new ExcelQuery<TSheet>(this.args);
        }

        public ExcelQuery<TSheet> Sheet<TSheet>(string sheetName)
        {
            this.args.SheetName = sheetName;
            return new ExcelQuery<TSheet>(this.args);
        }
    }
}
