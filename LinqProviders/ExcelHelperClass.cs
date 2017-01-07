using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqProviders
{
    public static class ExcelHelperClass
    {
        public static OleDbConnection GetConnection(ExcelQueryArgs args)
        {
            var connString = CreateConnectionString(args.FileName);
            return new OleDbConnection(connString);
        }

        public static IEnumerable<string> GetColumnNames(ExcelQueryArgs args)
        {
            var con = GetConnection(args);
            var res = new List<string>();
            try
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = $"SELECT TOP 1 * FROM [{args.SheetName}$]";
                    var data = cmd.ExecuteReader();
                    var scheme = data?.GetSchemaTable();
                    foreach (DataRow item in scheme.Rows)
                    {
                        res.Add(item["ColumnName"].ToString());
                    }
                }

                return res;
            }
            finally
            {
                con.Dispose();
            }
        }

        private static string CreateConnectionString(string filename)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            if (filename.EndsWith("xlsx"))
            {
                // XLSX - Excel 2007, 2010, 2012, 2013
                props["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
                props["Data Source"] = $"{filename}";
                props["Extended Properties"] = @"""Excel 12.0 XML;HDR=YES;IMEX=1""";
            }
            else if (filename.EndsWith("xls"))
            {
                // XLS - Excel 2003 and Older
                props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
                props["Data Source"] = $"{filename}";
                props["Extended Properties"] = @"""Excel 8.0 XML;HDR=YES;IMEX=1""";
            }
            var sb = new StringBuilder();

            foreach (var prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }

            return sb.ToString();
        }
    }
}
