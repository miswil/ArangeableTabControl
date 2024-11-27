using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoHideScrollViewer
{
    public class HorizontallyScrollWithMouseWheelBehavior : DependencyObject
    {
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(HorizontallyScrollWithMouseWheelBehavior),
                new PropertyMetadata(false, EnabledChanged));

        private static void EnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer) { return; }
            var enabled = (bool)e.NewValue;
            if (enabled)
            {
                scrollViewer.AddHandler(UIElement.MouseWheelEvent, (MouseWheelEventHandler)ScrollViewer_MouseWheel, handledEventsToo: true);
            }
            else
            {
                scrollViewer.RemoveHandler(UIElement.MouseWheelEvent, (MouseWheelEventHandler)ScrollViewer_MouseWheel);
            }
        }

        private static void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Delta < 0)
                {
                    scrollViewer.LineRight();
                }
                else
                {
                    scrollViewer.LineLeft();
                }
            }
        }
    }
}
