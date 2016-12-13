using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using System.Collections.Immutable;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Composition;
using System.Windows;
using System.Globalization;
using Microsoft.CodeAnalysis.Scripting;

namespace LinqPad.Editor
{
    public sealed class RoslynEditorHost
    {
        private int programId  = 0;
        private readonly MefHostServices host;
        private LinqPadResolver resolver;
        private readonly CompositionContext compositionContext;
        private readonly CSharpParseOptions parseOptions =
            new CSharpParseOptions(
                languageVersion: LanguageVersion.CSharp6,
                documentationMode: DocumentationMode.Parse,
                kind: SourceCodeKind.Script);

        private static readonly ImmutableArray<Type> defaultReferenceAssemblyTypes = new[] {
            typeof(object),
            typeof(Thread),
            typeof(Task),
            typeof(List<>),
            typeof(Regex),
            typeof(StringBuilder),
            typeof(Uri),
            typeof(Enumerable),
            typeof(IEnumerable),
            typeof(Path),
            typeof(Assembly),
            typeof(LinqPadExtensions),
        }.ToImmutableArray();

        private static readonly ImmutableArray<Assembly> defaultReferenceAssemblies =
            defaultReferenceAssemblyTypes.Select(x => x.Assembly).Distinct().Concat(new[]
            {
                Assembly.Load("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly,
            }).ToImmutableArray();

        public ImmutableArray<MetadataReference> DefaultReferences { get; }
        public ImmutableArray<string>            DefaultImports    { get; }
        //#r in current script document
        public IList<string>                     ResolveReferences { get; }


        private readonly ConcurrentDictionary<DocumentId, LinqPadWorkspace> workspaces
            = new ConcurrentDictionary<DocumentId, LinqPadWorkspace>();


        private CSharpCompilationOptions CreateCompilationOptions()
        {
            resolver = new LinqPadResolver(new string[] { "C:\\" }.ToImmutableArray(), "C:\\");
            var options = new CSharpCompilationOptions(
                outputKind: OutputKind.NetModule,
                usings: DefaultImports,
                metadataReferenceResolver: resolver);
            return options;
        }



        //Constructor
        public RoslynEditorHost(IEnumerable<Assembly> addAssemblies = null)
        {
            DefaultReferences = defaultReferenceAssemblies.Select(t => 
                CreateMetadataReference(t.Location)).ToImmutableArray();

            DefaultImports = defaultReferenceAssemblyTypes.
                Select(x => x.Namespace).Distinct().
                ToImmutableArray();

            ResolveReferences = new List<string>();

            var assemblies = new[]
            {
                Assembly.Load("Microsoft.CodeAnalysis"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
            };

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()
                    .SelectMany(x => x.GetTypes())
                    .ToArray();

            compositionContext = new ContainerConfiguration()
                            .WithParts(partTypes)
                            .CreateContainer();

            host = MefHostServices.Create(compositionContext);
        }

        private MetadataReference CreateMetadataReference(string location)
        {
            return MetadataReference.CreateFromFile(
                path: location);
        }

        public DocumentId AddDocument(LinqPadSourceTextContainer container)
        {
            var workspace = new LinqPadWorkspace(host: host, roslynEditorHost: this);
            var project = CreateProject(workspace.CurrentSolution);
            var document = CreateDocument(
                workspace: workspace,
                project: project,
                textContainer: container);

            var a = document.GetTextAsync().Result.Container;
            workspaces.TryAdd(document.Id, workspace);
            return document.Id;
        }

        public Document GetDocument(DocumentId documentId)
        {
            LinqPadWorkspace workspace;
            return workspaces.TryGetValue(documentId, out workspace) ?
                workspace.CurrentSolution.GetDocument(documentId) : null;
        }

        public TService GetService<TService>()
        {
            return compositionContext.GetExport<TService>();
        }

        public Project CreateProject(Solution solution)
        {
            var name = "Program " + programId++;
            var projectInfo = ProjectInfo.Create(
                id: ProjectId.CreateNewId(name),
                version: VersionStamp.Create(),
                name: name,
                assemblyName: name,
                language: LanguageNames.CSharp,
                parseOptions: parseOptions,
                metadataReferences: DefaultReferences,
                compilationOptions: CreateCompilationOptions());

            solution = solution.AddProject(projectInfo);
            return solution.GetProject(projectInfo.Id);
        }

        public Document CreateDocument(LinqPadWorkspace workspace, LinqPadSourceTextContainer textContainer, Project project)
        {
            var id = DocumentId.CreateNewId(project.Id);
            var solution = project.Solution.AddDocument(id, project.Name, textContainer.CurrentText);
            workspace.SetCurrentSolution(solution);
            workspace.OpenDocument(id, textContainer);
            return solution.GetDocument(id);
        }

        public void ProcessResolveReferences(DocumentId id, IEnumerable<string> references)
        {
            var oldProject = GetDocument(id).Project;
            LinqPadWorkspace workspace;
            if (workspaces.TryGetValue(id, out workspace))
            {
                
            }
        }
    }

    public sealed class LinqPadWorkspace : Workspace
    {
        private readonly RoslynEditorHost roslynEditorHost;
        private DocumentId openDocumentId;

        public LinqPadWorkspace(HostServices host, RoslynEditorHost roslynEditorHost) : base(host, WorkspaceKind.Host)
        {
            this.roslynEditorHost = roslynEditorHost;
        }

        public void OpenDocument(DocumentId documentId, SourceTextContainer container)
        {
            openDocumentId = documentId;
            OnDocumentOpened(documentId, container);
            OnDocumentContextUpdated(documentId);
        }

        public new void SetCurrentSolution(Solution solution)
        {
            var oldSolution = CurrentSolution;
            var newSolution = base.SetCurrentSolution(solution);
            RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionChanged, oldSolution, newSolution);
        }
    }

    public sealed class LinqPadResolver : MetadataReferenceResolver
    {
        private readonly ScriptMetadataResolver resolver;

        public LinqPadResolver(ImmutableArray<string> searchPaths, string baseDirectory)
        {
            resolver = ScriptMetadataResolver.Default.
                WithSearchPaths(searchPaths).
                WithBaseDirectory(baseDirectory);
        }

        public override bool ResolveMissingAssemblies => resolver.ResolveMissingAssemblies;

        public override bool Equals(object other)
        {
            return resolver.Equals(other);
        }

        public override int GetHashCode()
        {
            return resolver.GetHashCode();
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            return resolver.ResolveReference(reference, baseFilePath, properties);
        }
    }
}
