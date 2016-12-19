using ICSharpCode.AvalonEdit;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LinqPad.Execution
{
    public sealed class ScriptRunner
    {
        private ScriptOptions scriptOptions =
            ScriptOptions.Default;

        public event Action<IList<ResultObject>> Dumped;
        private Dispatcher dispatcher;
        private ConcurrentQueue<ResultObject> queue;

        private List<string> references;
        private List<string> imports;
        public ScriptRunner(IEnumerable<string> references, IEnumerable<string> imports)
        {
            this.references = references.ToList();
            this.imports = imports.ToList();
            scriptOptions = scriptOptions.
                WithReferences(references).
                WithImports(imports);
            LinqPadExtensions.Dumped += LinqPadExtensions_Dumped;
            queue = new ConcurrentQueue<ResultObject>();

            using (var resetEvent = new ManualResetEventSlim(false))
            {
                var uiThread = new Thread(() =>
                {
                    dispatcher = Dispatcher.CurrentDispatcher;
                    resetEvent.Set();
                    Dispatcher.Run();
                });
                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.IsBackground = true;
                uiThread.Start();
                resetEvent.Wait();
            }
        }

        private void LinqPadExtensions_Dumped(object arg1, string arg2)
        {
            queue.Enqueue(new ResultObject(arg1, arg2));
        }

        public async Task ExecuteAsync(string code, CancellationToken token)
        {
            var script = CSharpScript.Create(code).
                WithOptions(scriptOptions);

            await (await dispatcher.InvokeAsync(async () =>
            {
                await script.RunAsync(cancellationToken: token).ConfigureAwait(false);
                await DequeResultObjects(token).ConfigureAwait(false);
            },
            priority: DispatcherPriority.SystemIdle,
            cancellationToken: token)).ConfigureAwait(false);
        }

        private Task DequeResultObjects(CancellationToken token)
        {
            return Task.Run(() =>
            {
                ResultObject item;
                var list = new List<ResultObject>();
                while (queue.TryDequeue(out item) && !token.IsCancellationRequested)
                {
                    list.Add(item);
                }
                Dumped?.Invoke(list);
            });
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
