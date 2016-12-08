using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostScript
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = CSharpScript.Create("2.Dump()").
                WithOptions(ScriptOptions.Default.
                    WithReferences(typeof(Program).Assembly).
                    WithImports("HostScript"));

            var b = a.RunAsync().Result;
        }
    }

    public static class DumpObj
    {
        public static T Dump<T>(this T obj, string nameval = "")
        {
            Dumped?.Invoke(obj, nameval);
            return obj;
        }

        public static event Action<object, string> Dumped;
    }
}
