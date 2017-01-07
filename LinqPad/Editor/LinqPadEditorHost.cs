using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;

namespace LinqPad.Editor
{
    public sealed class LinqPadEditorHost
    {
        private static readonly ImmutableArray<Type> DefaultReferenceAssemblyTypes =
            new[]
                    {
                        typeof(object), typeof(Thread), typeof(Task), typeof(List<>), typeof(Regex),
                        typeof(StringBuilder), typeof(Uri), typeof(Enumerable), typeof(IEnumerable),
                        typeof(Path), typeof(Assembly), typeof(LinqPadExtensions), typeof(OxyPlot.PlotModel),
                        typeof(OxyPlot.Series.Series)
                    }
                .ToImmutableArray();

        private static readonly ImmutableArray<Assembly> DefaultReferenceAssemblies =
            DefaultReferenceAssemblyTypes.Select(x => x.Assembly).Distinct().Concat(new[]
            {
                Assembly.Load("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly,
            }).ToImmutableArray();

        private readonly MefHostServices host;

        private readonly CompositionContext compositionContext;

        private readonly CSharpParseOptions parseOptions = new CSharpParseOptions(
            languageVersion: LanguageVersion.Latest,
            documentationMode: DocumentationMode.Parse,
            kind: SourceCodeKind.Script);

        private readonly ConcurrentDictionary<DocumentId, LinqPadWorkspace> workspaces
             = new ConcurrentDictionary<DocumentId, LinqPadWorkspace>();

        private int programId;
        
        private LinqPadResolver resolver;

        public LinqPadEditorHost(IEnumerable<Assembly> addAssemblies = null)
        {
            this.DefaultReferences = DefaultReferenceAssemblies.Select(t => 
                CreateMetadataReference(t.Location)).ToImmutableArray();

            this.DefaultImports = DefaultReferenceAssemblyTypes.Select(x => x.Namespace).Distinct().ToImmutableArray();

            this.ResolveReferences = new List<string>();

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

            this.compositionContext = new ContainerConfiguration()
                            .WithParts(partTypes)
                            .CreateContainer();

            this.host = MefHostServices.Create(this.compositionContext);
        }

        public ImmutableArray<MetadataReference> DefaultReferences { get; }

        public ImmutableArray<string> DefaultImports { get; }

        // #r in current script document
        public IList<string> ResolveReferences { get; }

        public DocumentId AddDocument(LinqPadSourceTextContainer container)
        {
            var workspace = new LinqPadWorkspace(host: this.host, roslynEditorHost: this);
            var project = this.CreateProject(workspace.CurrentSolution);
            var document = CreateDocument(
                workspace: workspace,
                project: project,
                textContainer: container);

            var a = document.GetTextAsync().Result.Container;
            this.workspaces.TryAdd(document.Id, workspace);
            return document.Id;
        }

        public Document GetDocument(DocumentId documentId)
        {
            LinqPadWorkspace workspace;
            return this.workspaces.TryGetValue(documentId, out workspace) ?
                workspace.CurrentSolution.GetDocument(documentId) : null;
        }


        private static Document CreateDocument(LinqPadWorkspace workspace, LinqPadSourceTextContainer textContainer, Project project)
        {
            var id = DocumentId.CreateNewId(project.Id);
            var solution = project.Solution.AddDocument(id, project.Name, textContainer.CurrentText);
            workspace.SetCurrentSolution(solution);
            workspace.OpenDocument(id, textContainer);
            return solution.GetDocument(id);
        }

        private static MetadataReference CreateMetadataReference(string location)
        {
            return MetadataReference.CreateFromFile(
                path: location);
        }

        private Project CreateProject(Solution solution)
        {
            var name = "Program " + this.programId++;
            var projectInfo = ProjectInfo.Create(
                id: ProjectId.CreateNewId(name),
                version: VersionStamp.Create(),
                name: name,
                assemblyName: name,
                language: LanguageNames.CSharp,
                parseOptions: this.parseOptions,
                metadataReferences: this.DefaultReferences,
                compilationOptions: this.CreateCompilationOptions());

            solution = solution.AddProject(projectInfo);
            return solution.GetProject(projectInfo.Id);
        }

        private CSharpCompilationOptions CreateCompilationOptions()
        {
            this.resolver = new LinqPadResolver(new string[] { "C:\\" }.ToImmutableArray(), "C:\\");
            var options = new CSharpCompilationOptions(
                outputKind: OutputKind.NetModule,
                usings: this.DefaultImports,
                metadataReferenceResolver: this.resolver);
            return options;
        }
    }

    public sealed class LinqPadWorkspace : Workspace
    {
        private readonly LinqPadEditorHost roslynEditorHost;
        private DocumentId openDocumentId;

        public LinqPadWorkspace(HostServices host, LinqPadEditorHost roslynEditorHost) : base(host, WorkspaceKind.Host)
        {
            this.roslynEditorHost = roslynEditorHost;
        }

        public void OpenDocument(DocumentId documentId, SourceTextContainer container)
        {
            this.openDocumentId = documentId;
            this.OnDocumentOpened(documentId, container);
            this.OnDocumentContextUpdated(documentId);
        }

        public new void SetCurrentSolution(Solution solution)
        {
            var oldSolution = this.CurrentSolution;
            var newSolution = base.SetCurrentSolution(solution);
            this.RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionChanged, oldSolution, newSolution);
        }
    }

    public sealed class LinqPadResolver : MetadataReferenceResolver
    {
        private readonly ScriptMetadataResolver resolver;

        public LinqPadResolver(ImmutableArray<string> searchPaths, string baseDirectory)
        {
            this.resolver = ScriptMetadataResolver.Default.WithSearchPaths(searchPaths).WithBaseDirectory(baseDirectory);
        }

        public override bool ResolveMissingAssemblies => this.resolver.ResolveMissingAssemblies;

        public override bool Equals(object other)
        {
            return this.resolver.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.resolver.GetHashCode();
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(
            string reference,
            string baseFilePath,
            MetadataReferenceProperties properties)
        {
            return this.resolver.ResolveReference(reference, baseFilePath, properties);
        }
    }
}
