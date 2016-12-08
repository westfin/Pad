using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.Editor
{
    public sealed class ToolTipArgs
    {
        public bool InDocument { get; set; }
        public TextLocation LogicalPosition { get; set; }
        public int Position { get; set; }
        public object ContentToShow { get; set; }
        public void SetToolTip(object content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
                ContentToShow = content;
        }
    }
}
