using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ArrangeableTabControlLib
{
    internal class TabItemArrangingAdorner : Adorner
    {
        private Brush? brush;
        private readonly FrameworkElement draggedItem;

        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(
                "Position",
                typeof(Point),
                typeof(TabItemArrangingAdorner),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        public Point Offset
        {
            get { return (Point)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Offset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(
                "Offset",
                typeof(Point),
                typeof(TabItemArrangingAdorner),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        public TabItemArrangingAdorner(
            FrameworkElement draggedItem,
            ArrangeableTabControl tabControl,
            Point position, Point offset) : base(tabControl)
        {
            this.draggedItem = draggedItem;
            this.Position = position;
            this.Offset = offset;
            this.brush = new VisualBrush(this.draggedItem);
            //var dpi = 120;
            //DrawingVisual draw = new DrawingVisual();
            //using (DrawingContext drawingContext = draw.RenderOpen())
            //{
            //    var brush = new VisualBrush { Visual = this.draggedItem, Stretch = Stretch.None };
            //    var rect = new Rect(new Size(this.draggedItem.RenderSize.Width, this.draggedItem.RenderSize.Height));
            //    drawingContext.DrawRectangle(brush, null, rect);
            //}
            //RenderTargetBitmap bmp = new RenderTargetBitmap((int)(this.draggedItem.RenderSize.Width * dpi / 96), (int)(this.draggedItem.RenderSize.Height * dpi / 96), dpi, dpi, PixelFormats.Default);
            //bmp.Render(draw);
            //this.brush = new ImageBrush { ImageSource = bmp, Stretch = Stretch.None };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pt = new Point(this.Position.X + this.Offset.X, this.Position.Y + this.Offset.Y);
            var rect = new Rect(pt, new Size(this.draggedItem.RenderSize.Width, this.draggedItem.RenderSize.Height));
            drawingContext.DrawRectangle(this.brush, null, rect);
        }
    }
}
