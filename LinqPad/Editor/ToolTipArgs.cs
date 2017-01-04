using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit.Document;

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
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.ContentToShow = content;
        }
    }
}
