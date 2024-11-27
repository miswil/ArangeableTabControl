using System.Windows;
using System.Windows.Controls;

namespace ArrangeableTabControlLib
{
    public class UniformStackPanel : StackPanel
    {
        public double MaxLength
        {
            get { return (double)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(double), typeof(UniformStackPanel), new PropertyMetadata(0.0));

        protected override Size MeasureOverride(Size constratint)
        {
            if (this.InternalChildren.Count == 0)
            {
                return new Size(0, 0);
            }
            if (this.Orientation == Orientation.Horizontal)
            {
                Size childSize = new Size(this.MaxLength, constratint.Height);
                if (this.InternalChildren.Count * this.MaxLength > constratint.Width)
                {
                    childSize.Width = constratint.Width / this.InternalChildren.Count;
                }
                foreach (var child in this.InternalChildren)
                {
                    ((UIElement)child).Measure(childSize);
                }
                var children = this.InternalChildren.Cast<UIElement>();
                return new Size(children.Select(ui => ui.DesiredSize.Width).Sum(), children.Select(ui => ui.DesiredSize.Height).Max());
            }
            else
            {
                Size childSize = new Size(constratint.Width, this.MaxLength);
                if (this.InternalChildren.Count * this.MaxLength > constratint.Height)
                {
                    childSize.Height = constratint.Height / this.InternalChildren.Count;
                }
                foreach (var child in this.InternalChildren)
                {
                    ((UIElement)child).Measure(childSize);
                }
                var children = this.InternalChildren.Cast<UIElement>();
                return new Size(children.Select(ui => ui.DesiredSize.Width).Max(), children.Select(ui => ui.DesiredSize.Height).Sum());
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.InternalChildren.Count == 0)
            {
                return new Size(0, 0);
            }
            if (this.Orientation == Orientation.Horizontal)
            {
                Size childSize = new Size(this.MaxLength, arrangeSize.Height);
                if (this.InternalChildren.Count * this.MaxLength > arrangeSize.Width)
                {
                    childSize.Width = arrangeSize.Width / this.InternalChildren.Count;
                }
                var offset = 0.0;
                foreach (var child in this.InternalChildren)
                {
                    ((UIElement)child).Arrange(new Rect(new Point(offset, 0.0), childSize));
                    offset += childSize.Width;
                }
                return new Size(arrangeSize.Width, this.InternalChildren.Cast<UIElement>().Select(ui => ui.RenderSize.Height).Max());
            }
            else
            {
                Size childSize = new Size(arrangeSize.Width, this.MaxLength);
                if (this.InternalChildren.Count * this.MaxLength > arrangeSize.Height)
                {
                    childSize.Height = arrangeSize.Height / this.InternalChildren.Count;
                }
                var offset = 0.0;
                foreach (var child in this.InternalChildren)
                {
                    ((UIElement)child).Arrange(new Rect(new Point(0.0, offset), childSize));
                    offset += childSize.Height;
                }
                return new Size(this.InternalChildren.Cast<UIElement>().Select(ui => ui.RenderSize.Width).Max(), arrangeSize.Height);
            }
        }
    }
}
