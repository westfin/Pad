using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.Editor
{
    public static class ExtensionsCompletionItem
    {
        private static readonly ImmutableDictionary<string, ImmutableDictionary<string, Glyph>> Dictionary = InitializeDictionary();

        public static Glyph? GetGlyph(this CompletionItem completionItem)
        {
            var tags = completionItem.Tags;
            for (var index = 0; index < tags.Length; index++)
            {
                var tag = tags[index];
                var inner = Dictionary.GetValueOrDefault(tag);
                if (inner != null)
                {
                    Glyph glyph;
                    if (inner.TryGetValue(string.Empty, out glyph) ||
                        (index + 1 < tags.Length && inner.TryGetValue(tags[index + 1], out glyph)))
                    {
                        return glyph;
                    }
                }
            }

            return null;
        }

        private static ImmutableDictionary<string, ImmutableDictionary<string, Glyph>> InitializeDictionary()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, Glyph>>();
            foreach (var glyph in (Glyph[])Enum.GetValues(typeof(Glyph)))
            {
                var tags = GlyphTags.GetTags(glyph);
                if (tags.IsDefaultOrEmpty)
                {
                    continue;
                }

                var firstTag = tags[0];
                var secondTag = tags.Length == 2 ? tags[1] : string.Empty;
                var inner = builder.GetValueOrDefault(firstTag) ??
                            ImmutableDictionary<string, Glyph>.Empty.Add(secondTag, glyph);

                builder[firstTag] = inner.SetItem(secondTag, glyph);
            }

            return builder.ToImmutable();
        }
    }

    // Get from roslyn http://source.roslyn.io/#Microsoft.CodeAnalysis.Features/Completion/GlyphTags.cs,b0e3e21f6ebe226d
    public static class GlyphTags
    {
        public static ImmutableArray<string> GetTags(Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.Assembly:
                    return WellKnownTagArrays.Assembly;
                case Glyph.BasicFile:
                    return WellKnownTagArrays.VisualBasicProject;
                case Glyph.BasicProject:
                    return WellKnownTagArrays.VisualBasicProject;
                case Glyph.ClassPublic:
                    return WellKnownTagArrays.ClassPublic;
                case Glyph.ClassProtected:
                    return WellKnownTagArrays.ClassProtected;
                case Glyph.ClassPrivate:
                    return WellKnownTagArrays.ClassPrivate;
                case Glyph.ClassInternal:
                    return WellKnownTagArrays.ClassInternal;
                case Glyph.CSharpFile:
                    return WellKnownTagArrays.CSharpFile;
                case Glyph.CSharpProject:
                    return WellKnownTagArrays.CSharpProject;
                case Glyph.ConstantPublic:
                    return WellKnownTagArrays.ConstantPublic;
                case Glyph.ConstantProtected:
                    return WellKnownTagArrays.ConstantProtected;
                case Glyph.ConstantPrivate:
                    return WellKnownTagArrays.ConstantPrivate;
                case Glyph.ConstantInternal:
                    return WellKnownTagArrays.ConstantInternal;
                case Glyph.DelegatePublic:
                    return WellKnownTagArrays.DelegatePublic;
                case Glyph.DelegateProtected:
                    return WellKnownTagArrays.DelegateProtected;
                case Glyph.DelegatePrivate:
                    return WellKnownTagArrays.DelegateProtected;
                case Glyph.DelegateInternal:
                    return WellKnownTagArrays.DelegateInternal;
                case Glyph.EnumPublic:
                    return WellKnownTagArrays.EnumPublic;
                case Glyph.EnumProtected:
                    return WellKnownTagArrays.EnumProtected;
                case Glyph.EnumPrivate:
                    return WellKnownTagArrays.EnumPrivate;
                case Glyph.EnumInternal:
                    return WellKnownTagArrays.EnumInternal;
                case Glyph.EnumMember:
                    return WellKnownTagArrays.EnumMember;
                case Glyph.Error:
                    return WellKnownTagArrays.Error;
                case Glyph.StatusInformation:
                    return WellKnownTagArrays.StatusInformation;
                case Glyph.EventPublic:
                    return WellKnownTagArrays.EventPublic;
                case Glyph.EventProtected:
                    return WellKnownTagArrays.EventProtected;
                case Glyph.EventPrivate:
                    return WellKnownTagArrays.EventPrivate;
                case Glyph.EventInternal:
                    return WellKnownTagArrays.EventInternal;
                case Glyph.ExtensionMethodPublic:
                    return WellKnownTagArrays.ExtensionMethodPublic;
                case Glyph.ExtensionMethodProtected:
                    return WellKnownTagArrays.ExtensionMethodProtected;
                case Glyph.ExtensionMethodPrivate:
                    return WellKnownTagArrays.ExtensionMethodPrivate;
                case Glyph.ExtensionMethodInternal:
                    return WellKnownTagArrays.ExtensionMethodInternal;
                case Glyph.FieldPublic:
                    return WellKnownTagArrays.FieldPublic;
                case Glyph.FieldProtected:
                    return WellKnownTagArrays.FieldProtected;
                case Glyph.FieldPrivate:
                    return WellKnownTagArrays.FieldPrivate;
                case Glyph.FieldInternal:
                    return WellKnownTagArrays.FieldInternal;
                case Glyph.InterfacePublic:
                    return WellKnownTagArrays.InterfacePublic;
                case Glyph.InterfaceProtected:
                    return WellKnownTagArrays.InterfaceProtected;
                case Glyph.InterfacePrivate:
                    return WellKnownTagArrays.InterfacePrivate;
                case Glyph.InterfaceInternal:
                    return WellKnownTagArrays.InterfaceInternal;
                case Glyph.Intrinsic:
                    return WellKnownTagArrays.Intrinsic;
                case Glyph.Keyword:
                    return WellKnownTagArrays.Keyword;
                case Glyph.Label:
                    return WellKnownTagArrays.Label;
                case Glyph.Local:
                    return WellKnownTagArrays.Local;
                case Glyph.Namespace:
                    return WellKnownTagArrays.Namespace;
                case Glyph.MethodPublic:
                    return WellKnownTagArrays.MethodPublic;
                case Glyph.MethodProtected:
                    return WellKnownTagArrays.MethodProtected;
                case Glyph.MethodPrivate:
                    return WellKnownTagArrays.ModulePrivate;
                case Glyph.MethodInternal:
                    return WellKnownTagArrays.MethodInternal;
                case Glyph.ModulePublic:
                    return WellKnownTagArrays.ModulePublic;
                case Glyph.ModuleProtected:
                    return WellKnownTagArrays.ModuleProtected;
                case Glyph.ModulePrivate:
                    return WellKnownTagArrays.ModulePrivate;
                case Glyph.ModuleInternal:
                    return WellKnownTagArrays.ModuleInternal;
                case Glyph.OpenFolder:
                    return WellKnownTagArrays.Folder;
                case Glyph.Operator:
                    return WellKnownTagArrays.Operator;
                case Glyph.Parameter:
                    return WellKnownTagArrays.Parameter;
                case Glyph.PropertyPublic:
                    return WellKnownTagArrays.PropertyPublic;
                case Glyph.PropertyProtected:
                    return WellKnownTagArrays.PropertyProtected;
                case Glyph.PropertyPrivate:
                    return WellKnownTagArrays.PropertyPrivate;
                case Glyph.PropertyInternal:
                    return WellKnownTagArrays.PropertyInternal;
                case Glyph.RangeVariable:
                    return WellKnownTagArrays.RangeVariable;
                case Glyph.Reference:
                    return WellKnownTagArrays.Reference;
                case Glyph.StructurePublic:
                    return WellKnownTagArrays.StructurePublic;
                case Glyph.StructureProtected:
                    return WellKnownTagArrays.StructureProtected;
                case Glyph.StructurePrivate:
                    return WellKnownTagArrays.StructurePrivate;
                case Glyph.StructureInternal:
                    return WellKnownTagArrays.StructureInternal;
                case Glyph.TypeParameter:
                    return WellKnownTagArrays.TypeParameter;
                case Glyph.Snippet:
                    return WellKnownTagArrays.Snippet;
                case Glyph.CompletionWarning:
                    return WellKnownTagArrays.Warning;
                case Glyph.AddReference:
                    return WellKnownTagArrays.AddReference;
                case Glyph.NuGet:
                    return WellKnownTagArrays.NuGet;
                case Glyph.None:
                default:
                    return ImmutableArray<string>.Empty;
            }
        }
    }

    public static class WellKnownTagArrays
    {
        public static readonly ImmutableArray<string> Assembly                  = ImmutableArray.Create(WellKnownTags.Assembly);
        public static readonly ImmutableArray<string> ClassPublic               = ImmutableArray.Create(WellKnownTags.Class, WellKnownTags.Public);
        public static readonly ImmutableArray<string> ClassProtected            = ImmutableArray.Create(WellKnownTags.Class, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> ClassPrivate              = ImmutableArray.Create(WellKnownTags.Class, WellKnownTags.Private);
        public static readonly ImmutableArray<string> ClassInternal             = ImmutableArray.Create(WellKnownTags.Class, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> ConstantPublic            = ImmutableArray.Create(WellKnownTags.Constant, WellKnownTags.Public);
        public static readonly ImmutableArray<string> ConstantProtected         = ImmutableArray.Create(WellKnownTags.Constant, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> ConstantPrivate           = ImmutableArray.Create(WellKnownTags.Constant, WellKnownTags.Private);
        public static readonly ImmutableArray<string> ConstantInternal          = ImmutableArray.Create(WellKnownTags.Constant, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> DelegatePublic            = ImmutableArray.Create(WellKnownTags.Delegate, WellKnownTags.Public);
        public static readonly ImmutableArray<string> DelegateProtected         = ImmutableArray.Create(WellKnownTags.Delegate, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> DelegatePrivate           = ImmutableArray.Create(WellKnownTags.Delegate, WellKnownTags.Private);
        public static readonly ImmutableArray<string> DelegateInternal          = ImmutableArray.Create(WellKnownTags.Delegate, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> EnumPublic                = ImmutableArray.Create(WellKnownTags.Enum, WellKnownTags.Public);
        public static readonly ImmutableArray<string> EnumProtected             = ImmutableArray.Create(WellKnownTags.Enum, WellKnownTags.Public);
        public static readonly ImmutableArray<string> EnumPrivate               = ImmutableArray.Create(WellKnownTags.Enum, WellKnownTags.Private);
        public static readonly ImmutableArray<string> EnumInternal              = ImmutableArray.Create(WellKnownTags.Enum, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> EnumMember                = ImmutableArray.Create(WellKnownTags.EnumMember);
        public static readonly ImmutableArray<string> EventPublic               = ImmutableArray.Create(WellKnownTags.Event, WellKnownTags.Public);
        public static readonly ImmutableArray<string> EventProtected            = ImmutableArray.Create(WellKnownTags.Event, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> EventPrivate              = ImmutableArray.Create(WellKnownTags.Event, WellKnownTags.Private);
        public static readonly ImmutableArray<string> EventInternal             = ImmutableArray.Create(WellKnownTags.Event, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> ExtensionMethodPublic     = ImmutableArray.Create(WellKnownTags.ExtensionMethod, WellKnownTags.Public);
        public static readonly ImmutableArray<string> ExtensionMethodProtected  = ImmutableArray.Create(WellKnownTags.ExtensionMethod, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> ExtensionMethodPrivate    = ImmutableArray.Create(WellKnownTags.ExtensionMethod, WellKnownTags.Private);
        public static readonly ImmutableArray<string> ExtensionMethodInternal   = ImmutableArray.Create(WellKnownTags.ExtensionMethod, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> FieldPublic               = ImmutableArray.Create(WellKnownTags.Field, WellKnownTags.Public);
        public static readonly ImmutableArray<string> FieldProtected            = ImmutableArray.Create(WellKnownTags.Field, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> FieldPrivate              = ImmutableArray.Create(WellKnownTags.Field, WellKnownTags.Private);
        public static readonly ImmutableArray<string> FieldInternal             = ImmutableArray.Create(WellKnownTags.Field, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> InterfacePublic           = ImmutableArray.Create(WellKnownTags.Interface, WellKnownTags.Public);
        public static readonly ImmutableArray<string> InterfaceProtected        = ImmutableArray.Create(WellKnownTags.Interface, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> InterfacePrivate          = ImmutableArray.Create(WellKnownTags.Interface, WellKnownTags.Private);
        public static readonly ImmutableArray<string> InterfaceInternal         = ImmutableArray.Create(WellKnownTags.Interface, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> Intrinsic                 = ImmutableArray.Create(WellKnownTags.Intrinsic);
        public static readonly ImmutableArray<string> Keyword                   = ImmutableArray.Create(WellKnownTags.Keyword);
        public static readonly ImmutableArray<string> Label                     = ImmutableArray.Create(WellKnownTags.Label);
        public static readonly ImmutableArray<string> Local                     = ImmutableArray.Create(WellKnownTags.Local);
        public static readonly ImmutableArray<string> Namespace                 = ImmutableArray.Create(WellKnownTags.Namespace);
        public static readonly ImmutableArray<string> MethodPublic              = ImmutableArray.Create(WellKnownTags.Method, WellKnownTags.Public);
        public static readonly ImmutableArray<string> MethodProtected           = ImmutableArray.Create(WellKnownTags.Method, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> MethodPrivate             = ImmutableArray.Create(WellKnownTags.Method, WellKnownTags.Private);
        public static readonly ImmutableArray<string> MethodInternal            = ImmutableArray.Create(WellKnownTags.Method, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> ModulePublic              = ImmutableArray.Create(WellKnownTags.Module, WellKnownTags.Public);
        public static readonly ImmutableArray<string> ModuleProtected           = ImmutableArray.Create(WellKnownTags.Module, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> ModulePrivate             = ImmutableArray.Create(WellKnownTags.Module, WellKnownTags.Private);
        public static readonly ImmutableArray<string> ModuleInternal            = ImmutableArray.Create(WellKnownTags.Module, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> Folder                    = ImmutableArray.Create(WellKnownTags.Folder);
        public static readonly ImmutableArray<string> Operator                  = ImmutableArray.Create(WellKnownTags.Operator);
        public static readonly ImmutableArray<string> Parameter                 = ImmutableArray.Create(WellKnownTags.Parameter);
        public static readonly ImmutableArray<string> PropertyPublic            = ImmutableArray.Create(WellKnownTags.Property, WellKnownTags.Public);
        public static readonly ImmutableArray<string> PropertyProtected         = ImmutableArray.Create(WellKnownTags.Property, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> PropertyPrivate           = ImmutableArray.Create(WellKnownTags.Property, WellKnownTags.Private);
        public static readonly ImmutableArray<string> PropertyInternal          = ImmutableArray.Create(WellKnownTags.Property, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> RangeVariable             = ImmutableArray.Create(WellKnownTags.RangeVariable);
        public static readonly ImmutableArray<string> Reference                 = ImmutableArray.Create(WellKnownTags.Reference);
        public static readonly ImmutableArray<string> StructurePublic           = ImmutableArray.Create(WellKnownTags.Structure, WellKnownTags.Public);
        public static readonly ImmutableArray<string> StructureProtected        = ImmutableArray.Create(WellKnownTags.Structure, WellKnownTags.Protected);
        public static readonly ImmutableArray<string> StructurePrivate          = ImmutableArray.Create(WellKnownTags.Structure, WellKnownTags.Private);
        public static readonly ImmutableArray<string> StructureInternal         = ImmutableArray.Create(WellKnownTags.Structure, WellKnownTags.Internal);
        public static readonly ImmutableArray<string> TypeParameter             = ImmutableArray.Create(WellKnownTags.TypeParameter);
        public static readonly ImmutableArray<string> Snippet                   = ImmutableArray.Create(WellKnownTags.Snippet);

        public static readonly ImmutableArray<string> Error                     = ImmutableArray.Create(WellKnownTags.Error);
        public static readonly ImmutableArray<string> Warning                   = ImmutableArray.Create(WellKnownTags.Warning);
        public static readonly ImmutableArray<string> StatusInformation         = ImmutableArray.Create(WellKnownTags.StatusInformation);

        public static readonly ImmutableArray<string> AddReference              = ImmutableArray.Create(WellKnownTags.AddReference);
        public static readonly ImmutableArray<string> NuGet                     = ImmutableArray.Create(WellKnownTags.NuGet);

        public static readonly ImmutableArray<string> CSharpFile                = ImmutableArray.Create(WellKnownTags.File, LanguageNames.CSharp);
        public static readonly ImmutableArray<string> VisualBasicFile           = ImmutableArray.Create(WellKnownTags.File, LanguageNames.VisualBasic);

        public static readonly ImmutableArray<string> CSharpProject             = ImmutableArray.Create(WellKnownTags.Project, LanguageNames.CSharp);
        public static readonly ImmutableArray<string> VisualBasicProject        = ImmutableArray.Create(WellKnownTags.Project, LanguageNames.VisualBasic);
    }

    public static class WellKnownTags
    {
        // accessibility
        public static readonly string Public            = nameof(Public);
        public static readonly string Protected         = nameof(Protected);
        public static readonly string Private           = nameof(Private);
        public static readonly string Internal          = nameof(Internal);

        // project elements
        public static readonly string File              = nameof(File);
        public static readonly string Project           = nameof(Project);
        public static readonly string Folder            = nameof(Folder);
        public static readonly string Assembly          = nameof(Assembly);

        // language elements
        public static readonly string Class             = nameof(Class);
        public static readonly string Constant          = nameof(Constant);
        public static readonly string Delegate          = nameof(Delegate);
        public static readonly string Enum              = nameof(Enum);
        public static readonly string EnumMember        = nameof(EnumMember);
        public static readonly string Event             = nameof(Event);
        public static readonly string ExtensionMethod   = nameof(ExtensionMethod);
        public static readonly string Field             = nameof(Field);
        public static readonly string Interface         = nameof(Interface);
        public static readonly string Intrinsic         = nameof(Intrinsic);
        public static readonly string Keyword           = nameof(Keyword);
        public static readonly string Label             = nameof(Label);
        public static readonly string Local             = nameof(Local);
        public static readonly string Namespace         = nameof(Namespace);
        public static readonly string Method            = nameof(Method);
        public static readonly string Module            = nameof(Module);
        public static readonly string Operator          = nameof(Operator);
        public static readonly string Parameter         = nameof(Parameter);
        public static readonly string Property          = nameof(Property);
        public static readonly string RangeVariable     = nameof(RangeVariable);
        public static readonly string Reference         = nameof(Reference);
        public static readonly string Structure         = nameof(Structure);
        public static readonly string TypeParameter     = nameof(TypeParameter);

        // other
        public static readonly string Snippet           = nameof(Snippet);
        public static readonly string Error             = nameof(Error);
        public static readonly string Warning           = nameof(Warning);

        public static readonly string StatusInformation = nameof(StatusInformation);

        public static readonly string AddReference      = nameof(AddReference);
        public static readonly string NuGet             = nameof(NuGet);
    }
}
