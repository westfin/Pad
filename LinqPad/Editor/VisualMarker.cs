using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LinqPad.Editor
{
    public sealed class VisualMarker : TextSegment
    {
        private readonly LnqPadColorizerService service;

        private Color markerColor;

        public VisualMarker(LnqPadColorizerService service, int startOffset, int length)
        {
            this.service = service;
            StartOffset = startOffset;
            Length = length;
        }

        public void Delete()
        {
            service.Remove(this);
        }

        public Color MarkerColor
        {
            get { return markerColor; }
            set
            {
                if (markerColor == value)
                    return;
                markerColor = value;
                service.Redraw(this);
            }
        }

        public object ToolTip { get; set; }
    }
}
