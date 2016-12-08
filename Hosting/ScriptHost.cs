using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Host
{
    public sealed class ScriptHost
    {
        private static readonly string hostPath = "LinqPad.Host.exe";
        private static readonly ManualResetEventSlim clientExited =
            new ManualResetEventSlim(false);

        private readonly IEnumerable<string> references;
        private readonly IEnumerable<string> imports;
        private RemotionService remotionService;

        public ScriptHost()
        {
        }

        public string ExecuteString(string code)
        {
            if (remotionService == null)
                remotionService = TryStartProcess();

            return remotionService.Service.GetString(code);
        }


        public void Execute(string code)
        {
            if (remotionService == null)
                remotionService = TryStartProcess();

            remotionService.Service.ExecuteScript(code);
        }

        public RemotionService TryStartProcess()
        {
            Process process = null;
            var remoteServerPort = "HostChannel-" + Guid.NewGuid();

            var processInfo = new ProcessStartInfo(hostPath)
            {
                Arguments = $"{remoteServerPort} {Process.GetCurrentProcess().Id}",
                CreateNoWindow = true,
                UseShellExecute = true,
            };

            try
            {
                process = new Process() { StartInfo = processInfo };
                process.Start();

                var pipe = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
                {
                    ReceiveTimeout = TimeSpan.MaxValue,
                    MaxReceivedMessageSize = Int32.MaxValue,
                    SendTimeout = TimeSpan.MaxValue
                };

                try
                {
                    var service = ChannelFactory<IScriptService>.CreateChannel(
                         binding: pipe,
                         endpointAddress: new EndpointAddress(new UriBuilder
                         {
                             Scheme = Uri.UriSchemeNetPipe,
                             Path = remoteServerPort
                         }.Uri));

                    //service.InitAsync(references, imports);
                    return new RemotionService(process, service);
                }
                catch (CommunicationException e)
                {
                    return null;
                }

            }
            catch (InvalidOperationException e)
            {
                return null;
            }
        }

        public static void Run(string port, int processId)
        {
            if (!IsAttachProcess(processId))
            {
                return;
            }

            ServiceHost host = null;

            host = new ServiceHost(typeof(ScriptService));
            var pipe = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxReceivedMessageSize = Int32.MaxValue,
                SendTimeout = TimeSpan.MaxValue
            };

            host.AddServiceEndpoint(
                implementedContract: typeof(IScriptService),
                binding: pipe,
                address: new UriBuilder { Scheme = Uri.UriSchemeNetPipe, Path = port }.
                Uri);

            host.Open();
            clientExited.Wait();
        }

        private static bool IsAttachProcess(int processId)
        {
            Process process;
            try
            {
                process = Process.GetProcessById(processId);
                return true;
            }
            catch (ArgumentException e)
            {
                return false;
            }
        }
    }

    public sealed class RemotionService
    {
        public IScriptService Service { get; }
        public Process Process { get; }

        public RemotionService(Process process, IScriptService service)
        {
            Service = service;
            Process = process;
        }
    }

    [ServiceContract]
    public interface IScriptService
    {
        [OperationContract]
        void ExecuteScript(string code);

        [OperationContract]
        Task InitAsync(IEnumerable<string> references, IEnumerable<string> imports);

        [OperationContract]
        string GetString(string code);

    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true)]
    public class ScriptService : IScriptService
    {

        private ScriptOptions scriptOptions =
            ScriptOptions.Default;

        private readonly ParseOptions parseOptions =
            new CSharpParseOptions(
                languageVersion: LanguageVersion.CSharp6,
                documentationMode: DocumentationMode.Parse,
                kind: SourceCodeKind.Script);

        public void ExecuteScript(string code)
        {
            var a = CSharpScript.Create(code);
            var b = a.RunAsync();
        }

        public Task InitAsync(IEnumerable<string> references, IEnumerable<string> imports)
        {
            scriptOptions = scriptOptions.
                WithReferences(references).
                WithImports(imports);
            return Task.CompletedTask;
        }

        public string GetString(string code)
        {
            return code;
        }
    }
}
