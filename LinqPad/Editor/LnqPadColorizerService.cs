using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LinqPad.Editor
{
    public sealed class LnqPadColorizerService : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;
        private readonly TextEditor editor;
        private readonly TextSegmentCollection<VisualMarker> markers;


        public void Remove(VisualMarker marker)
        {
            if (marker == null)
                throw new ArgumentNullException(nameof(marker));
            markers.Remove(marker);
        }

        public LnqPadColorizerService(TextEditor editor)
        {
            this.editor = editor;
            markers = new TextSegmentCollection<VisualMarker>(editor.Document);
        }

        public VisualMarker TryAdd(int start, int length)
        {
            var textLength = editor.Document.TextLength;
            if (start < 0 || length > textLength)
                return null;

            if (length < 0 || start + length > textLength)
                return null;
            var marker = new VisualMarker(this, start, length);
            markers.Add(marker);
            return marker;
        }

        public void Clear()
        {
            markers.Clear();
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView == null)
                throw new ArgumentNullException(nameof(textView));
            if (drawingContext == null)
                throw new ArgumentNullException(nameof(drawingContext));

            int start = textView.VisualLines.First().FirstDocumentLine.Offset;
            int end   = textView.VisualLines.Last().LastDocumentLine.Offset;
            foreach (var marker in markers.FindOverlappingSegments(start, end - start))
            {
                var brush = new SolidColorBrush(marker.MarkerColor);
                brush.Freeze();
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var startPoint = rect.BottomLeft;
                    var endPoint   = rect.BottomRight;

                    var offset = 2.5;
                    var count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);
                    var geometry = new StreamGeometry();
                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, offset, count).ToArray(), true, false);
                    }
                    geometry.Freeze();
                    var usedPen = new Pen(brush, 1);
                    usedPen.Freeze();
                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                }
            }
        }

        public void Redraw(ISegment segment)
        {
            editor.TextArea.TextView.Redraw(segment);
        }

        private static IEnumerable<Point> CreatePoints(Point start, double offset, int count)
        {
            for (var i = 0; i < count; i++)
                yield return new Point(start.X + i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0));
        }

        public IEnumerable<VisualMarker> GetMarkersAtOffset(int offset)
        {
            if (markers == null)
                return Enumerable.Empty<VisualMarker>();
            return markers.FindSegmentsContaining(offset);
        }
    }
}
