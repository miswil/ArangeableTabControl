using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArrangeableTabControlLib
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ArrangeableTabControlLib"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ArrangeableTabControlLib;assembly=ArrangeableTabControlLib"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:ArrangeableTabItem/>
    ///
    /// </summary>
    public class ArrangeableTabItem : TabItem
    {
        static ArrangeableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArrangeableTabItem), new FrameworkPropertyMetadata(typeof(ArrangeableTabItem)));
        }

        public static RoutedEvent ArrangeStartedEvent = EventManager.RegisterRoutedEvent(
            nameof(ArrangeStartedEvent),
            RoutingStrategy.Bubble,
            typeof(MouseButtonEventHandler),
            typeof(ArrangeableTabItem));

        public static RoutedEvent ArrangeRequiredEvent = EventManager.RegisterRoutedEvent(
            nameof(ArrangeRequiredEvent),
            RoutingStrategy.Bubble,
            typeof(MouseEventHandler),
            typeof(ArrangeableTabItem));

        public static RoutedEvent ArrangeCompletedEvent = EventManager.RegisterRoutedEvent(
            nameof(ArrangeCompletedEvent),
            RoutingStrategy.Bubble,
            typeof(MouseButtonEventHandler),
            typeof(ArrangeableTabItem));

        internal FrameworkElement GetContentHost() { return (FrameworkElement)this.GetTemplateChild("Border"); }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.RaiseEvent(
                new MouseButtonEventArgs((MouseDevice)e.Device, e.Timestamp, e.ChangedButton)
                {
                    RoutedEvent = ArrangeStartedEvent,
                    Source = this,
                });
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (e.LeftButton == MouseButtonState.Released) { return; }
            this.RaiseEvent(
                new MouseEventArgs((MouseDevice)e.Device, e.Timestamp)
                {
                    RoutedEvent = ArrangeRequiredEvent,
                    Source = this,
                });
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.RaiseEvent(
                new MouseButtonEventArgs((MouseDevice)e.Device, e.Timestamp, e.ChangedButton)
                {
                    RoutedEvent = ArrangeCompletedEvent,
                    Source = this,
                });
        }
    }
}
