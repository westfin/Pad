using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace LinqPad.Editor
{
    public sealed class LnqPadColorizerService : IBackgroundRenderer
    {
        private readonly TextEditor editor;
        private readonly TextSegmentCollection<VisualMarker> markers;

        public LnqPadColorizerService(TextEditor editor)
        {
            this.editor = editor;
            this.markers = new TextSegmentCollection<VisualMarker>(editor.Document);
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void Remove(VisualMarker marker)
        {
            if (marker == null)
            {
                throw new ArgumentNullException(nameof(marker));
            }

            this.markers.Remove(marker);
        }

        public VisualMarker TryAdd(int start, int length)
        {
            var textLength = this.editor.Document.TextLength;
            if (start < 0 || length > textLength)
            {
                return null;
            }

            if (length < 0 || start + length > textLength)
            {
                return null;
            }

            var marker = new VisualMarker(this, start, length);
            this.markers.Add(marker);
            return marker;
        }

        public void Clear()
        {
            this.markers.Clear();
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView == null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            if (drawingContext == null)
            {
                throw new ArgumentNullException(nameof(drawingContext));
            }

            var start = textView.VisualLines.First().FirstDocumentLine.Offset;
            var end   = textView.VisualLines.Last().LastDocumentLine.Offset;
            foreach (var marker in this.markers.FindOverlappingSegments(start, end - start))
            {
                var brush = new SolidColorBrush(marker.MarkerColor);
                brush.Freeze();
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var startPoint = rect.BottomLeft;
                    var endPoint   = rect.BottomRight;

                    const double Offset = 2.5;
                    var count = Math.Max((int)((endPoint.X - startPoint.X) / Offset) + 1, 4);
                    var geometry = new StreamGeometry();
                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, Offset, count).ToArray(), true, false);
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
            this.editor.TextArea.TextView.Redraw(segment);
        }

        public IEnumerable<VisualMarker> GetMarkersAtOffset(int offset)
        {
            return this.markers == null ? Enumerable.Empty<VisualMarker>() : this.markers.FindSegmentsContaining(offset);
        }

        private static IEnumerable<Point> CreatePoints(Point start, double offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var x = start.X + (i * offset);
                var y = start.Y - ((i + 1) % 2 == 0 ? offset : 0);
                yield return new Point(x, y);
            }
        }
    }
}
