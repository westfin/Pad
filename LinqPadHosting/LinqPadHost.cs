using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinqPadHosting
{
    public sealed class LinqPadHost
    {
        private static readonly ManualResetEventSlim clientExited =
            new ManualResetEventSlim(false);

        private string hostPath = @"C:\Users\Ivan\Documents\Visual Studio 2015\Projects\LinqPad\LinqPad.Host\bin\Debug\LinqPad.Host.exe";

        private ScriptOptions scriptOptions =
            ScriptOptions.Default;

        private List<string> imports;
        private List<string> references;
        private ILinqPadService service;

        public LinqPadHost(IEnumerable<string> imports, IEnumerable<string> references)
        {
            this.imports = imports.ToList();
            this.references = references.ToList();
        }

        public static void RunHost(string serverPort, int processId)
        {
            if(!AttachToClientProcess(processId))
            {
                return;
            }
            ServiceHost serviceHost = null;
            try
            {
                serviceHost = new ServiceHost(typeof(LinqPadService));
                var binding = CreateBinding();
                var address = CreateUri(serverPort);

                serviceHost.AddServiceEndpoint(typeof(ILinqPadService), binding, address);
                serviceHost.Open();

                clientExited.Wait();
            }
            finally
            {
                if (serviceHost != null)
                {
                    serviceHost.Close();
                }
            }
        }

        public async Task ExecuteAsync(string code, CancellationToken token)
        {
            if (service == null)
            {
                service = StartProcess(token);
            }
            await service.ExecuteAsync(code);
        }

        private ILinqPadService StartProcess(CancellationToken cancellationToken)
        {
            Process process = null;
            try
            {
                int currenProcessId = Process.GetCurrentProcess().Id;
                var serverPort = "LinqPadHost-" + Guid.NewGuid();

                var processInfo = new ProcessStartInfo(hostPath)
                {
                    Arguments = $"{serverPort} {currenProcessId}",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                process = new Process()
                {
                    StartInfo = processInfo
                };

                process.Start();
                //cancellationToken.ThrowIfCancellationRequested();

                var binding = CreateBinding();
                var address = CreateUri(serverPort);
                service = ChannelFactory<ILinqPadService>.CreateChannel(binding, new EndpointAddress(address));
                service.Initialize(imports, references);
                return service;
            }
            finally
            {

            }
        }

        private static NetNamedPipeBinding CreateBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = Int32.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = TimeSpan.MaxValue
            };
        }

        private static Uri CreateUri(string port)
        {
            return new UriBuilder()
            {
                Scheme = Uri.UriSchemeNetPipe,
                Path = port
            }.Uri;
        }

        private static bool AttachToClientProcess(int clientProcessId)
        {
            Process clientProcess;
            try
            {
                clientProcess = Process.GetProcessById(clientProcessId);
            }
            catch (ArgumentException)
            {
                return false;
            }

            clientProcess.EnableRaisingEvents = true;
            clientProcess.Exited += new EventHandler((_, __) =>
            {
                clientExited.Set();
            });

            return clientProcess.IsAlive();
        }
    }
    
    [ServiceContract]
    public interface ILinqPadService
    {
        [OperationContract]
        Task ExecuteAsync(string code);
        [OperationContract]
        Task Initialize(IEnumerable<string> imports, IEnumerable<string> references);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public sealed class LinqPadService : ILinqPadService
    {
        private ScriptOptions scriptOptions = ScriptOptions.Default;

        public async Task ExecuteAsync(string code)
        {
            var script = CSharpScript.Create(code).
                WithOptions(scriptOptions);
            await script.RunAsync();
        }

        public Task Initialize(IEnumerable<string> imports, IEnumerable<string> references)
        {
            scriptOptions = scriptOptions.
                WithImports(imports).
                WithReferences(references);

            return Task.CompletedTask;
        }
    }
}
