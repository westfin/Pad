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
    using System.Data;

    public class ExcelQueryExecutor : IQueryExecutor
    {
        private readonly ExcelQueryArgs args;
        private readonly ExcelQueryModelVisitor visitor;

        public ExcelQueryExecutor(ExcelQueryArgs args)
        {
            this.visitor = new ExcelQueryModelVisitor(args);
            this.args = args;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            this.visitor.VisitQueryModel(queryModel);
            var cmd  = this.visitor.BuildSqlQuery();

            var projector = this.GetSelectProj<T>(queryModel);
            cmd.Connection = ExcelHelperClass.GetConnection(this.args);
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

        private IEnumerable<object> GetDataObj(QueryModel queryModel, IDataReader data)
        {
            var results = new List<object>();
            while (data.Read())
            {
                var obj = Activator.CreateInstance(queryModel.MainFromClause.ItemType);
                var props = obj.GetType().GetProperties();
                var columns = ExcelHelperClass.GetColumnNames(this.args);
                foreach (var prop in props)
                {
                    var columnNameProp = prop.GetCustomAttribute<ExcelColumnAttribute>();
                    if (!columns.Contains(columnNameProp.ColumnName))
                    {
                        throw new MappingException(columnNameProp.ColumnName, this.args);
                    }

                    prop.SetValue(obj, data[columnNameProp.ColumnName]);
                }

                results.Add(obj);
            }
            return results;
        }

        private Func<object, T> GetSelectProj<T>(QueryModel queryModel)
        {
            Func<object, T> projector = (result) => (T)result;
            if (this.visitor.IsSingleResult)
            {
                return projector;
            }

            var proj = ProjectorBuildingExpressionTreeVisitor.BuildProjector<T>(queryModel.SelectClause.Selector);
            projector = (result) => proj(new ResultObjectMapping(queryModel.MainFromClause, result));

            return projector;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return this.ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ?
                this.ExecuteCollection<T>(queryModel).SingleOrDefault() :
                this.ExecuteScalar<T>(queryModel);
        }
    }
}
