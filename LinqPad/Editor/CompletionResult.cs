using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace LinqPad.Editor
{
    using System.Linq;

    public sealed class CompletionResult
    {
        public CompletionResult(IEnumerable<ICompletionData> data, TextSpan span)
        {
            this.CompletionList = data.ToList();
            this.CompletionSpan = span;
        }

        public List<ICompletionData> CompletionList { get; }

        public TextSpan CompletionSpan { get; }
    }
}