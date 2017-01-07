using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq;

namespace LinqProviders
{
    public class ExcelQuery<T> : QueryableBase<T>
    {
        private static IQueryExecutor CreateExecutor(ExcelQueryArgs args)
        {
            return new ExcelQueryExecutor(args);
        }

        public ExcelQuery(ExcelQueryArgs args) :
            base(CreateExecutor(args)) { }

        public ExcelQuery(IQueryProvider provider, Expression expression) :
            base(provider, expression) { }
    }
}
