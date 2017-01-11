using Remotion.Data.Linq.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using LinqProviders.Attributes;
using LinqProviders.Exceptions;
using System.Data.OleDb;

namespace LinqProviders
{
    public sealed class WhereClauseExpressionTree : ThrowingExpressionTreeVisitor
    {
        private readonly StringBuilder whereClause = new StringBuilder();

        private readonly ExcelQueryArgs args;

        private readonly List<OleDbParameter> listParams;

        public IEnumerable<OleDbParameter> Params => this.listParams;

        public string WhereClause => this.whereClause.ToString();

        public WhereClauseExpressionTree(ExcelQueryArgs args)
        {
            this.args = args;
            this.listParams = new List<OleDbParameter>();
            this.whereClause.Append("WHERE ");
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotImplementedException(visitMethod + "method is not implemented");
        }

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            var left  = expression.Left;
            var right = expression.Right;
            this.VisitExpression(left);
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                    this.whereClause.Append(" AND ");
                    break;
                case ExpressionType.Equal:
                    this.whereClause.Append(" = ");
                    break;
                case ExpressionType.GreaterThan:
                    this.whereClause.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.whereClause.Append(" >= ");
                    break;
                case ExpressionType.LessThan:
                    this.whereClause.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.whereClause.Append(" <= ");
                    break;
                case ExpressionType.NotEqual:
                    this.whereClause.Append(" <> ");
                    break;
                case ExpressionType.OrElse:
                    this.whereClause.Append(" OR ");
                    break;
                default:
                    throw new NotSupportedException($"{expression.NodeType.ToString()} statement is not supported");
            }

            this.VisitExpression(right);
            return expression;
        }

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            var prop = expression.Member as PropertyInfo;
            var columns = ExcelHelperClass.GetColumnNames(args);
            if (prop == null)
            {
                return expression;
            }

            var columnNameProp = prop.GetCustomAttribute<ExcelColumnAttribute>();
            if (!columns.Contains(columnNameProp.ColumnName))
            {
                throw new MappingException(columnNameProp.ColumnName, this.args);
            }

            this.whereClause.Append($" {columnNameProp.ColumnName} ");

            return expression;
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "Contains":
                    this.MethodInWhereClause(expression, "LIKE", "%{0}%");
                    break;

                case "StartsWith":
                    this.MethodInWhereClause(expression, "LIKE", "{0}%");
                    break;

                case "EndsWith":
                    this.MethodInWhereClause(expression, "LIKE", "%{0}");
                    break;

                case "Equals":
                    this.MethodInWhereClause(expression, "=", "{0}");
                    break;

                default:
                    break;
            }
            return expression;
        }

        private void MethodInWhereClause(MethodCallExpression expression, string stringOperator, string argument)
        {
            this.VisitExpression(expression.Object);
            this.whereClause.Append($" {stringOperator} ? ");
            var val = expression.Arguments.First().ToString().Replace("\"", string.Empty);
            val = string.Format(argument, val);
            this.listParams.Add(new OleDbParameter("?", val));
        }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            this.listParams.Add(new OleDbParameter("?", expression.Value));
            this.whereClause.Append($" ? ");
            return expression;
        }
    }
}
