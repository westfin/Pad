using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            #region
            //var tree = CSharpSyntaxTree.ParseText(@"#r ""C:\Users\Ivan\Documents\Visual Studio 2015\Projects\LinqRepl\LinqProviders\bin\Debug\LinqProviders.dll""");
            //Console.WriteLine(tree.GetRoot().ContainsDirectives);
            //var compilation = CSharpCompilation.Create("Asss", new[] { tree });
            //var model = compilation.GetSemanticModel(tree);
            //var root = tree.GetRoot() as CompilationUnitSyntax;
            //var directives = root?.GetDiagnostics();


            var tree1 = CSharpSyntaxTree.ParseText(@"class Someclass { }");
            Console.WriteLine(tree1.GetRoot().ContainsDirectives);
            var compilation1 = CSharpCompilation.Create("Asss", new[] { tree1 });
            var model1 = compilation1.GetSemanticModel(tree1);
            var root1 = tree1.GetRoot() as CompilationUnitSyntax;
            var directives1 = root1.GetDiagnostics();
            Console.ReadLine();
            #endregion
        }
    }
}
