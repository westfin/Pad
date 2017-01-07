using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqProviders.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable<object> list, Func<object, TResult> caster)
        {
            var results = new List<TResult>();
            foreach (var item in list)
                results.Add(caster(item));
            return results;
        }
    }
}
