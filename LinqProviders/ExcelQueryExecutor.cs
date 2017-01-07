using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Data.Linq;
using System.Data.OleDb;
using System.Linq.Expressions;
using System.Reflection;
using LinqProviders.Extensions;
using LinqProviders.Attributes;
using LinqProviders.Exceptions;

namespace LinqProviders
{
    public class ExcelQueryExecutor : IQueryExecutor
    {
        private readonly ExcelQueryArgs args;
        private readonly ExcelQueryModelVisitor visitor;

        public ExcelQueryExecutor(ExcelQueryArgs args)
        {
            visitor = new ExcelQueryModelVisitor(args);
            this.args = args;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            visitor.VisitQueryModel(queryModel);
            var cmd  = visitor.BuildSqlQuery();
            var projector = GetSelectProj<T>(queryModel);
            try
            {
                cmd.Connection = ExcelHelperClass.GetConnection(args);
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }

                cmd.Parameters.AddRange(this.visitor.Builder.Params.ToArray());

                var data         = cmd.ExecuteReader();
                var objData      = this.GetDataObj(queryModel, data);
                var resultObject = objData.Cast(projector);

                return resultObject;
            }
            catch (OleDbException ex)
            {
                throw ex;
            }
        }

        private IEnumerable<object> GetDataObj(QueryModel queryModel, OleDbDataReader data)
        {
            var results = new List<object>();
            while (data.Read())
            {
                var obj = Activator.CreateInstance(queryModel.MainFromClause.ItemType);
                var props = obj.GetType().GetProperties();
                var columns = ExcelHelperClass.GetColumnNames(args);
                foreach (var prop in props)
                {
                    var columnNameProp = prop.GetCustomAttribute<ExcelColumnAttribute>();
                    if (!columns.Contains(columnNameProp.ColumnName))
                    {
                        throw new MappingException(columnNameProp.ColumnName, args);
                    }

                    if (columnNameProp != null)
                    {
                        prop.SetValue(obj, data[columnNameProp.ColumnName]);
                    }
                }
                results.Add(obj);
            }
            return results;
        }

        private static Func<object, T> GetSelectProj<T>(QueryModel queryModel) 
        {
            var proj = ProjectorBuildingExpressionTreeVisitor.BuildProjector<T>(queryModel.SelectClause.Selector);
            Func<object, T> projector = (result) => proj(new ResultObjectMapping(queryModel.MainFromClause, result));
            return projector;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            throw new NotImplementedException();
        }
    }
}
