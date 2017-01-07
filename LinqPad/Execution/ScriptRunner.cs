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

using ICSharpCode.AvalonEdit;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace LinqPad.Execution
{
    using Microsoft.CodeAnalysis;

    public sealed class ScriptRunner
    {
        private readonly ConcurrentQueue<ResultObject> queue;

        private ScriptOptions scriptOptions = ScriptOptions.Default;

        private Dispatcher dispatcher;

        private List<string> references;

        private List<string> imports;

        public ScriptRunner(IEnumerable<string> refs, IEnumerable<string> imps)
        {
            this.references = refs.ToList();
            this.imports = imps.ToList();
            this.scriptOptions = this.scriptOptions.WithReferences(refs).WithImports(imps);
            LinqPadExtensions.Dumped += this.LinqPadExtensionsDumped;
            this.queue = new ConcurrentQueue<ResultObject>();

            using (var resetEvent = new ManualResetEventSlim(false))
            {
                var uiThread = new Thread(() =>
                {
                    this.dispatcher = Dispatcher.CurrentDispatcher;
                    resetEvent?.Set();
                    Dispatcher.Run();
                });
                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.IsBackground = true;
                uiThread.Start();
                resetEvent.Wait();
            }
        }

        public event Action<IList<ResultObject>> Dumped;

        public async Task ExecuteAsync(string code, CancellationToken token)
        {
            var script = CSharpScript.Create(code)
                .WithOptions(this.scriptOptions);

            await(await this.dispatcher.InvokeAsync(
                callback: async () =>
                    {
                        await script.RunAsync(cancellationToken: token).ConfigureAwait(false);
                        await this.DequeResultObjects(token).ConfigureAwait(false);
                    },
                priority: DispatcherPriority.SystemIdle,
                cancellationToken: token)).ConfigureAwait(false);
        }

        public Task Initialize(IEnumerable<string> refs, IEnumerable<string> imps)
        {
            this.scriptOptions = this.scriptOptions.WithReferences(refs).WithImports(imps);
            return Task.CompletedTask;
        }

        private void LinqPadExtensionsDumped(object arg1, string arg2)
        {
            this.queue.Enqueue(new ResultObject(arg1, arg2));
        }

        private Task DequeResultObjects(CancellationToken token)
        {
            return Task.Run(
            () =>
                {
                    ResultObject item;
                    var list = new List<ResultObject>();
                    while (this.queue.TryDequeue(out item) && !token.IsCancellationRequested)
                    {
                        list.Add(item);
                    }

                    this.Dumped?.Invoke(list);
                },
            token);
        }
    }
}
