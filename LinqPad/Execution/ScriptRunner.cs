using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LinqPad.Execution
{
    public sealed class ScriptRunner
    {
        private ScriptOptions scriptOptions =
            ScriptOptions.Default;

        private List<string> references;
        private List<string> imports;
        public ScriptRunner(IEnumerable<string> references, IEnumerable<string> imports)
        {
            this.references = references.ToList();
            this.imports = imports.ToList();
            scriptOptions = scriptOptions.
                WithReferences(references).
                WithImports(imports);
        }

        public async Task ExecuteAsync(string code)
        {
            try
            {
                var script = CSharpScript.Create(code).
                    WithOptions(scriptOptions);
                await script.RunAsync().ConfigureAwait(false);
            }
            catch (CompilationErrorException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public Task Initialize(IEnumerable<string> references, IEnumerable<string> imports)
        {
            scriptOptions = scriptOptions.
                WithReferences(references).
                WithImports(imports);

            return Task.CompletedTask;
        }
    }
}
