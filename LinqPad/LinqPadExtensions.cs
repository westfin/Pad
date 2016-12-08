using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad
{
    public static class LinqPadExtensions
    {
        public static T Dump<T>(this T obj, string nameval = "")
        {
            Dumped?.Invoke(obj, nameval);
            return obj;
        }

        public static event Action<object, string> Dumped;
    }
}
