using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqProviders
{
    public sealed class SqlBuilder
    {
        private readonly StringBuilder sb;


        public SqlBuilder()
        {
            this.Aggregate = "*";
            this.Params = new List<OleDbParameter>();
            this.sb = new StringBuilder();
        }

        public IEnumerable<OleDbParameter> Params { get; set; }

        public string SheetName { private get; set; }

        public string FromClause { get; set; }

        public string WhereClause { private get; set; }

        public string OrderByClause { get; set; }

        public bool IsCountResult { get; set; }

        private string Aggregate { get; set; }

        public override string ToString()
        {
            if (this.IsCountResult)
            {
                this.Aggregate = "COUNT(*)";
            }

            this.sb.Append($"SELECT {this.Aggregate} FROM [{this.SheetName}] {this.WhereClause}");
            return this.sb.ToString();
        }
    }
}
