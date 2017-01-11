using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using System.Data.OleDb;
using Remotion.Data.Linq.Collections;

namespace LinqProviders
{
    using Remotion.Data.Linq.Clauses.ResultOperators;
    using Remotion.FunctionalProgramming;

    public class ExcelQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly ExcelQueryArgs args;

        public SqlBuilder Builder { get; }

        public bool IsSingleResult { get; private set; }

        public ExcelQueryModelVisitor(ExcelQueryArgs args)
        {
            this.args = args;
            this.Builder = new SqlBuilder();
            if (args.SheetName != null)
            {
                this.Builder.SheetName = args.SheetName + "$";
            }
            else
            {
                this.Builder.SheetName = "Sheet1$";
            }

        }

        public OleDbCommand BuildSqlQuery()
        {
            var cmd = new OleDbCommand { CommandText = this.Builder.ToString() };
            return cmd;
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            this.VisitBodyClauses(queryModel.BodyClauses, queryModel);
            this.VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var whereVisitor = new WhereClauseExpressionTree(this.args);
            whereVisitor.VisitExpression(whereClause.Predicate);
            this.Builder.WhereClause = whereVisitor.WhereClause;
            this.Builder.Params      = whereVisitor.Params;
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var countResult = resultOperator as CountResultOperator;
            if (countResult != null)
            {
                this.Builder.IsCountResult = true;
                this.IsSingleResult = true;
            }

            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
        }

        protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
        {
            base.VisitBodyClauses(bodyClauses, queryModel);
        }
    }
}
