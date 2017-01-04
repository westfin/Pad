using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Document;

namespace LinqPad.Editor
{
    public sealed class VisualMarker : TextSegment
    {
        private readonly LnqPadColorizerService service;

        private Color markerColor;

        public VisualMarker(LnqPadColorizerService service, int startOffset, int length)
        {
            this.service = service;
            this.StartOffset = startOffset;
            this.Length = length;
        }

        public Color MarkerColor
        {
            get
            {
                return this.markerColor;
            }

            set
            {
                if (this.markerColor == value)
                {
                    return;
                }

                this.markerColor = value;
                this.service.Redraw(this);
            }
        }

        public object ToolTip { get; set; }

        public void Delete()
        {
            this.service.Remove(this);
        }
    }
}
