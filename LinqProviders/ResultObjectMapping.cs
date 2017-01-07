using Remotion.Data.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqProviders
{
    public class ResultObjectMapping
    {
        private readonly Dictionary<IQuerySource, object> resultObjectsBySource = new Dictionary<IQuerySource, object>();

        public ResultObjectMapping(IQuerySource querySource, object resultObject)
        {
            this.Add(querySource, resultObject);
        }

        public T GetObject<T>(IQuerySource source)
        {
            return (T)this.resultObjectsBySource[source];
        }

        public IEnumerator<KeyValuePair<IQuerySource, object>> GetEnumerator()
        {
            return this.resultObjectsBySource.GetEnumerator();
        }

        private void Add(IQuerySource querySource, object resultObject)
        {
            this.resultObjectsBySource.Add(querySource, resultObject);
        }
    }
}
